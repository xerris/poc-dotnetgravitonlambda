using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Lambda;
using System.Collections.Generic;

namespace DotNetGravitonLambda
{
    public class DotNetGravitonLambdaStack : Stack
    {
        internal DotNetGravitonLambdaStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // The code that defines your stack goes here

            //Stage setting for Deployment (Need to have Deploy = false in RestApiProps to configure the Stage
            string environment = "PRD";


            Architecture[] armArchitecture = new Architecture[1];
            armArchitecture[0] = Architecture.ARM_64;

            Architecture[] x8664Architecture = new Architecture[1];
            x8664Architecture[0] = Architecture.X86_64;

            //COMPUTE INFRASTRUCTURE
            //Lambda setup
            var simpleLambdaHandler = new Function(this, "simpleLambdaHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_CORE_3_1,
                FunctionName = "dotNetSimpleLambda",
                MemorySize = 512,
                Timeout = Duration.Seconds(50),
                Architectures = x8664Architecture,
                //Where to get the code
                Code = Code.FromAsset("Lambdas\\src\\Lambdas\\bin\\Debug\\netcoreapp3.1"),
                Handler = "Lambdas::Lambdas.Function::SimpleLambdaHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment
                }
            });


            var gravitonSimpleLambdaHandler2 = new Function(this, "gravitonSimpleLambdaHandler", new FunctionProps
            {
                Runtime = Runtime.DOTNET_CORE_3_1,
                FunctionName = "gravitonDotNetSimpleLambda",
                MemorySize = 512,
                Timeout = Duration.Seconds(50),
                Architectures = armArchitecture,
                Code = Code.FromAsset("Lambdas\\src\\Lambdas\\bin\\Debug\\netcoreapp3.1"),
                Handler = "Lambdas::Lambdas.Function::SimpleLambdaHandler",
                Environment = new Dictionary<string, string>
                {
                    ["ENVIRONMENT"] = environment
                }
            });


            //This is the name of the API in the APIGateway
            var api = new RestApi(this, "DotNetGravitonAPI", new RestApiProps
            {
                RestApiName = "dotNetGravitonAPI",
                Description = "This our DotNetGravitonAPI",
                Deploy = false
            });

            var deployment = new Deployment(this, "My Deployment", new DeploymentProps { Api = api });
            var stage = new Amazon.CDK.AWS.APIGateway.Stage(this, "stage name", new Amazon.CDK.AWS.APIGateway.StageProps
            {
                Deployment = deployment,
                StageName = environment
            });
            api.DeploymentStage = stage;

            //Lambda integrations
            var simpleLambdaIntegration = new LambdaIntegration(simpleLambdaHandler, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            });

            var gravitonSimpleLambdaIntegration2 = new LambdaIntegration(gravitonSimpleLambdaHandler2, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string>
                {
                    ["application/json"] = "{ \"statusCode\": \"200\" }"
                }
            });

            //It is up to you if you want to structure your lambdas in separate APIGateway APIs (RestApi)

            //Option 1: Adding at the top level of the APIGateway API
            // api.Root.AddMethod("POST", simpleLambdaIntegration);

            //Option 2: Or break out resources under one APIGateway API as follows
            var simpleResource = api.Root.AddResource("dotNetSimple");
            var gravitonSimpleResource = api.Root.AddResource("dotNetGravitonSimple");

            var simpleMethod = simpleResource.AddMethod("POST", simpleLambdaIntegration);
            var gravitonSimpleMethod2 = gravitonSimpleResource.AddMethod("POST", gravitonSimpleLambdaIntegration2);

            //Output results of the CDK Deployment
            new CfnOutput(this, "A Region:", new CfnOutputProps() { Value = this.Region });
            new CfnOutput(this, "B API Gateway API:", new CfnOutputProps() { Value = api.Url });
            string urlPrefix = api.Url.Remove(api.Url.Length - 1);
            new CfnOutput(this, "C DotNet Simple Lambda:", new CfnOutputProps() { Value = urlPrefix + simpleMethod.Resource.Path });
            new CfnOutput(this, "D DotNet Graviton Simple Lambda:", new CfnOutputProps() { Value = urlPrefix + gravitonSimpleMethod2.Resource.Path });


        }
    }
}