using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Text;
using System.Text.RegularExpressions;
using computer_vision_quickstart;


    // Add your Computer Vision subscription key and endpoint
    string subscriptionKey = "01b9a5cdb3d047a6b2689ef7cfd076a4";
    string endpoint = "https://membership-app.cognitiveservices.azure.com/";

    const string READ_TEXT_URL_IMAGE = "https://raw.githubusercontent.com/Azure-Samples/cognitive-services-sample-data-files/master/ComputerVision/Images/printed_text.jpg";


    Console.WriteLine("Azure Cognitive Services Computer Vision - .NET quickstart example");
    Console.WriteLine();

    ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);

    // Extract text (OCR) from a URL image using the Read API
    //ReadFileUrl(client, READ_TEXT_URL_IMAGE).Wait();

    var fileName = @"D:\POC\computer-vision-quickstart\Set8-B.jpg";

    ReadFileLocal(client, fileName).Wait();


    static ComputerVisionClient Authenticate(string endpoint, string key)
    {
        ComputerVisionClient client =
            new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            { Endpoint = endpoint };
        return client;
    }

    static async Task ReadFileUrl(ComputerVisionClient client, string urlFile)
    {
        Console.WriteLine("----------------------------------------------------------");
        Console.WriteLine("READ FILE FROM URL");
        Console.WriteLine();

        // Read text from URL
        var textHeaders = await client.ReadAsync(urlFile);
        // After the request, get the operation location (operation ID)
        string operationLocation = textHeaders.OperationLocation;
        Thread.Sleep(2000);

        // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
        // We only need the ID and not the full URL
        const int numberOfCharsInOperationId = 36;
        string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

        // Extract the text
        ReadOperationResult results;
        Console.WriteLine($"Extracting text from URL file {Path.GetFileName(urlFile)}...");
        Console.WriteLine();
        do
        {
            results = await client.GetReadResultAsync(Guid.Parse(operationId));
        }
        while ((results.Status == OperationStatusCodes.Running ||
            results.Status == OperationStatusCodes.NotStarted));

        var readResult = new StringBuilder();

         // Display the found text.
        Console.WriteLine();
        var textUrlFileResults = results.AnalyzeResult.ReadResults;
        foreach (ReadResult page in textUrlFileResults)
        {
            foreach (Line line in page.Lines)
            {
                readResult.Append(line.Text);
                Console.WriteLine(line.Text);
            }
        }
        Console.WriteLine();
        Console.WriteLine("----------------------------------------------------------");
        Console.WriteLine(readResult);
        Console.WriteLine("----------------------------------------------------------");
        Console.WriteLine();
    }

    static async Task ReadFileLocal(ComputerVisionClient client, string localFile)
    {
        Console.WriteLine("----------------------------------------------------------");
        Console.WriteLine("READ FILE FROM LOCAL");
        Console.WriteLine();

        // Read text from URL
        var textHeaders = await client.ReadInStreamAsync(File.OpenRead(localFile));
        // After the request, get the operation location (operation ID)
        string operationLocation = textHeaders.OperationLocation;
        Thread.Sleep(2000);

        // <snippet_extract_response>
        // Retrieve the URI where the recognized text will be stored from the Operation-Location header.
        // We only need the ID and not the full URL
        const int numberOfCharsInOperationId = 36;
        string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

        // Extract the text
        ReadOperationResult results;
        Console.WriteLine($"Reading text from local file {Path.GetFileName(localFile)}...");
        Console.WriteLine();
        do
        {
            results = await client.GetReadResultAsync(Guid.Parse(operationId));
        }
        while ((results.Status == OperationStatusCodes.Running ||
            results.Status == OperationStatusCodes.NotStarted));
        // </snippet_extract_response>

         var readResult = new StringBuilder();

        // <snippet_extract_display>
        // Display the found text.
        Console.WriteLine();
        var textUrlFileResults = results.AnalyzeResult.ReadResults;
        foreach (ReadResult page in textUrlFileResults)
        {
            foreach (Line line in page.Lines)
            {
                readResult.Append(line.Text + " ");
                Console.WriteLine(line.Text);
            }
        }

        var finalResult = readResult.ToString().RemoveSpecialCharacters();

        Console.WriteLine("----------------------------------------------------------");
        Console.WriteLine(finalResult);
        Console.WriteLine("----------------------------------------------------------");

        // Check if the card is new or old 
        if (finalResult.IndexOf("Name:") > 0 && finalResult.IndexOf("Expiry Date") > 0)
        {
            // New card front side.
            Console.WriteLine("New card front side.");
        }

        if (finalResult.IndexOf("Name:") > 0 && finalResult.IndexOf("Expiry Date") == -1)
        {
            // Old card front side.
            Console.WriteLine("Old card front side.");
        }

        if (finalResult.IndexOf("Issuing Place:") > 0)
        {
            // New card back side
            Console.WriteLine("New card back side");
        }

        if (finalResult.IndexOf("Sex:") > 0 && finalResult.IndexOf("Name:") == -1)
        {
            // Old card back side
            Console.WriteLine("Old card back side");
        }

        // var firstIndex = finalResult.IndexOf("Sex:");
        // var secondIndex = finalResult.IndexOf("Name:");
        // Console.WriteLine($"First index {firstIndex}, Second index {secondIndex}");


        // int firstStringPositionForEid = finalResult.IndexOf("ID Number ");    
        // string eidNo = finalResult.Substring(firstStringPositionForEid + 10, 18);    

        // int firstStringPositionForName = finalResult.IndexOf("Name:");    
        // int secondStringPositionForName = finalResult.IndexOf(":  Nationality:");    
        // string name = finalResult.Substring(firstStringPositionForName + 5, secondStringPositionForName - (firstStringPositionForName + 5));    

        // var dob = "";
        // var split = name.Split(":");
        
        // if (split.Length > 1)
        // {
        //     name = split[0];
        //     expiry = split[1].Substring(0, 8);
        // }

        // var expiry = "";
        // var cardNo = "";

        // int firstStringPositionForExpiry = finalResult.IndexOf("Card Number");

        // if (firstStringPositionForExpiry > 0) 
        // {
        //     expiry = finalResult.Substring(firstStringPositionForExpiry + 11, 23);
        //     var expirySplit = expiry.Split(" ");
        //     if (expirySplit.Length > 2)
        //     {
        //         expiry = expirySplit[1];
        //         cardNo = expirySplit[2];
        //     }
        // }

        // // string eidNo = finalResult.Substring(firstStringPositionForEid + 10, 18);  

        // int firstStringPositionForDob = finalResult.IndexOf(" Date of Birth");

        // if (firstStringPositionForDob > 0) 
        // {
        //     dob = finalResult.Substring(firstStringPositionForDob - 10, 10);
        // }

        // Console.WriteLine();
        // Console.WriteLine("----------------------------------------------------------");
        // Console.WriteLine(finalResult);
        // Console.WriteLine("----------------------------------------------------------");
        // // Console.WriteLine(eidNo);
        // // Console.WriteLine(name);
        // Console.WriteLine(dob);
        // Console.WriteLine(expiry);
        // Console.WriteLine(cardNo);
        // Console.WriteLine("----------------------------------------------------------");
        // Console.WriteLine();
    }
   
