#pragma warning disable MEAI001
using Microsoft.Agents.AI.Workflows;
using Shared;
using System.Text;
using Workflow.AiAssisted.PizzaSample;
using Workflow.AiAssisted.PizzaSample.Models;
using Workflow.AiAssisted.PizzaSample.Executors;

Secrets secrets = SecretManager.GetSecrets();
AgentFactory agentFactory = new(secrets);

PizzaOrderParserExecutor orderParser = new(agentFactory.CreateOrderTakerAgent());
PizzaStockCheckerExecutor stockChecker = new();
PizzaSuccessExecutor endSuccess = new();
PizzaWarningExecutor endWarning = new(agentFactory.CreateWarningToCustomerAgent());

WorkflowBuilder builder = new(orderParser);

builder.AddEdge(
    source: orderParser,
    target: stockChecker);

builder.AddSwitch(
    source: stockChecker,
    switchBuilder =>
    {
        switchBuilder.AddCase<PizzaOrder>(x => x!.Warnings.Count == 0, endSuccess);
        switchBuilder.AddCase<PizzaOrder>(x => x!.Warnings.Count != 0, endWarning);
    }
);

var workflow = builder.Build();

Console.OutputEncoding = Encoding.UTF8;

const string input = "Make a big Pepperoni Pizza with mushrooms and onions";

StreamingRun run = await InProcessExecution.StreamAsync(workflow: workflow, input: input);

await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is ExecutorCompletedEvent executorComplete)
    {
        Utils.WriteLineDarkGray($"{executorComplete.ExecutorId} Completed");
    }
}