using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Shared.AI;

public class OnnxLocalEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly InferenceSession _session;
    private readonly Tokenizer _tokenizer; 
    private const int ModelDimension = 384;

    public OnnxLocalEmbeddingGenerator(string modelPath, string vocabPath)
    {
        _session = new InferenceSession(modelPath);
        
        _tokenizer = BertTokenizer.Create(vocabPath);
    }

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, 
        EmbeddingGenerationOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        var embeddings = new List<Embedding<float>>();

        foreach (var text in values)
        {
            var ids = _tokenizer.EncodeToIds(text);
            var inputIds = ids.Select(t => (long)t).ToArray();
            var attentionMask = Enumerable.Repeat(1L, inputIds.Length).ToArray();
            var typeIds = Enumerable.Repeat(0L, inputIds.Length).ToArray();

            ReadOnlySpan<int> dimensions = new int[] { 1, inputIds.Length };

            var inputTensor = new DenseTensor<long>(inputIds.AsMemory(), dimensions);
            var maskTensor = new DenseTensor<long>(attentionMask.AsMemory(), dimensions);
            var typeTensor = new DenseTensor<long>(typeIds.AsMemory(), dimensions);

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", maskTensor),
                NamedOnnxValue.CreateFromTensor("token_type_ids", typeTensor)
            };

            using var results = _session.Run(inputs);
            
            var output = results.First().AsEnumerable<float>().ToArray();

            embeddings.Add(new Embedding<float>(output.Take(ModelDimension).ToArray()));
        }

        return await Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
    }

    public void Dispose()
    {
        _session.Dispose();
        GC.SuppressFinalize(this);
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;
}