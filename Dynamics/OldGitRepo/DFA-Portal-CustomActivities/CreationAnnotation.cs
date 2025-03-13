using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Text.Json;

namespace DFA_Portal_CustomActivities
{
    public class CreateAnnotation : CodeActivity
    {
            
        [RequiredArgument]
        [Input("signature")]
        public InArgument<string> signature { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                // Get the CRM service from the context
                IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
                IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
                IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);

                string jsonString = @"
        {
            ""id"": ""13B3159A-90BE-47B5-AEED-2B5ACB95DBF5"",
 ""FileName"": ""John"",
 ""ContentType"": ""John"",
            ""Regarding"": 30,
            ""Content"": ""iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAACmklEQVR42mJwwBv4DAbHgZGd3I0TDhkgviDfY1Zi9/A2Zm5xbkH5RlebD6TZkHJweUqLlQ1TVlXZGNqblE1uUJMeA8ICvPE3fQrRy96MEpPXy1Och7e5yj46ysI+/OT2ZmggYUFHiX/hbliNPS9atTayIhAiEhCAiYYgKxDZWEFBQnHf4Xx2tpc3PzGznjn+NQGQysIIiBxkZuYaF3f3Wg1Ws8ZMTMxs3EczPz1ByAII5T3nAdYhXmCyyALW5re3M+u9OJjEb5jn/xxrGYwJiDfWbAI9/87PKzMhwyMZubWWLEAIxMc/j2vz8/PzQDY8fz3OzshDg8PD7/zsNBykZw9eN7t9HZvnjFQ0NCLp6crJy7gfv5+7du3M8+KSt8Dki8yMz5+fvzq1+/aDQQAvPAUCzs7O4uLi5PlOXMjIyMJXb+DbjZ2d3szM/Pz9RUdHC6XEp8DQKSkpHBxMUFGRnZ7e3svd3jkdHR0mIOQYznsbGx0TlZ2fm5uaPHhyn7T2x3z5saGkqKiogeUdDDqVlZWpfe/n1atXK/5OTlJubm7/Dx8fHoWD4BlB6+Ph4THDd3d3XC5XKUwQAmZmJpGdwn2Z2dHf39/nw4dptgjs8ODlpv3rUODv3r2XK9cu8xIYvXr1UpzFwAA4b+JtQ7zJxcXKsm4ABhRIAxUjXubWkkmvPhozNzcx+/vp5syZgEev359gOZmNiYT5+/pk+jbf5I7QOec7fxVgYAn/zhTp69atfn5+fgY8cORn5ysqKvzsm5ubBc1X6PiYOjq6h3+/QphfP09PT48vjjeeDEiIqKqra2NgaA3eHKjEyMEymT58uWxb/AWZwRrl27vT6f/MENIY+wfAJlL24gbNHTlxwQHN6ATYdO3e25q6uuPy5hYWEwj+fPNzY2NnEycnf+vn5+fMZ9vT2DI0dHRy8vLzz4gICBA6OjoT0VDAICRFAjkP1QEYmJiDzeBfz1GAJv4D7KgST1ERETWFgYOTk5MjzxDx/buZsaGUz8AOAh44bz4Bx7zwCoKA9Y3ABaAw4Od8ICPLykY6VhDEYj/AAAAAElFTkSuQmCC""
        }";
                string id = ExtractValue(jsonString, @"""id"":");
                string FileName = ExtractValue(jsonString, @"""FileName"":");
                int regard = int.Parse(ExtractValue(jsonString, @"""Regarding"":"));
                string carsBase64 = ExtractValue(jsonString, @"""Content"":");
                byte[] carsImage = Convert.FromBase64String(carsBase64);
                //using (JsonDocument document = JsonDocument.Parse(jsonString))
                //{
                //    JsonElement root = document.RootElement;

                //    // Access the "name" property
                //    string name = root.GetProperty("FileName").GetString();
                //    string id = root.GetProperty("id").GetString();

                //    // Access the "age" property
                //    int age = root.GetProperty("Regarding").GetInt32();

                //    // Access the "cars" property as a Base64 string
                //    string carsBase64 = root.GetProperty("Content").GetString();

                //    // Convert the Base64 string to a byte array
                //    byte[] carsImage2 = Convert.FromBase64String(carsBase64);


                var app = new Entity("dfa_appapplication", new Guid(id));
                    // Retrieve input parameters
                    var sign = signature.Get(context);

                    Entity annotation = new Entity("annotation");
                    annotation["subject"] = "test";
                    annotation["notetext"] = "Annotation description"; // You can add an optional description here.
                    annotation["objectid"] = app;
                    annotation["documentbody"] = carsImage;
                    annotation["filename"] = sign;
                    annotation["mimetype"] = "image/png"; // Replace with the appropriate MIME type for your file.

                    // Create the annotation in CRM
                    service.Create(annotation);
                    // Now you can use the "cars" image byte array as needed
                   // Console.WriteLine($"Image Size: {carsImage.Length} bytes");
              
                // Deserialize the JSON string into a Person object
             

                // Access the "cars" byte array (image)
              

               
                // Create the annotation entity
               
            }
            catch (Exception ex)
            {
                // Handle any exceptions here, e.g., logging or throwing a fault to stop the workflow
                throw new InvalidWorkflowException($"Error creating annotation: {ex.Message}");
            }
        }
        private static string ExtractValue(string jsonString, string propertyName)
        {
            int startIndex = jsonString.IndexOf(propertyName) + propertyName.Length;
            int endIndex = jsonString.IndexOf(',', startIndex);
            if (endIndex == -1)
                endIndex = jsonString.IndexOf('}', startIndex);

            if (startIndex == -1 || endIndex == -1)
                throw new ArgumentException($"Property {propertyName} not found in the JSON string.");

            string value = jsonString.Substring(startIndex, endIndex - startIndex).Trim(' ', '"');

            // Remove any whitespace characters from the value
            value = value.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");

            return value;
        }
    }
    public class dfa_signature
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string Regarding { get; set; }
        public string id { get; set; }
    }

}
