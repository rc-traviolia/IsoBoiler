using Microsoft.Azure.Functions.Worker;
using Moq;

namespace IsoBoiler.Testing
{
    public static class FunctionContextMother
    {
        public static FunctionContext Birth(string functionName = "ExampleFunctionName")
        {
            var functionDefinitionMock = new Mock<FunctionDefinition>();
            functionDefinitionMock.Setup(fd => fd.Name).Returns(functionName);

            var functionContextMock = new Mock<FunctionContext>();
            functionContextMock.Setup(ctx => ctx.FunctionDefinition).Returns(functionDefinitionMock.Object);

            return functionContextMock.Object;
        }
    }
}
