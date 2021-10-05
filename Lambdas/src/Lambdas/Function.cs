using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambdas
{
    public class Function
    {

        /* 
              *  Sample input json body to submit for this Lambda
              *  
              *  {
                   "anyjson":"somevalue"
                 }
              */
        public object SimpleLambdaHandler(object input, ILambdaContext context)
        {
            //basic elements of our response
            string message = "";
            bool success = true;
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now;
            TimeSpan duration = TimeSpan.Zero;
            try
            {

                //perform CPU workload
                startTime = DateTime.Now;
                executeCPUIntensiveTask();
                endTime = DateTime.Now;
                duration = endTime - startTime;

            }
            catch (Exception exc)
            {
                message += "SimpleLambdaHandler Exception:" + exc.Message + "," + exc.StackTrace;
                success = false;
            }

            //create the responseBody for the response
            string responseBody = "{\n";
            responseBody += " \"startTime\":" + startTime + ",\n";
            responseBody += " \"endTime\":\"" + endTime + "\",\n";
            responseBody += " \"duration\":\"" + duration + "\",\n";
            responseBody += " \"success\":\"" + success + "\",\n";
            responseBody += " \"message\":\"" + message + "\",\n";
            responseBody += "}";

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = responseBody,
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };
            return response;
        }

        private static void executeCPUIntensiveTask()
        {
            //some cpu intensive task
            long limit = 5000;
            String lastSqRoot = "";
            for (int i = 0; i < limit; i++)
            {
                for (int j = 0; j < limit; j++)
                {
                    double sqroot = Math.Sqrt(j);
                    lastSqRoot = "" + sqroot;
                }
            }
            Console.WriteLine("lastSqRoot=" + lastSqRoot);
        }


    }
}