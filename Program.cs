﻿using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;


    // Add your Computer Vision subscription key and endpoint
    string subscriptionKey = "01b9a5cdb3d047a6b2689ef7cfd076a4";
    string endpoint = "https://membership-app.cognitiveservices.azure.com/";

    const string READ_TEXT_URL_IMAGE = "https://raw.githubusercontent.com/Azure-Samples/cognitive-services-sample-data-files/master/ComputerVision/Images/printed_text.jpg";


    Console.WriteLine("Azure Cognitive Services Computer Vision - .NET quickstart example");
    Console.WriteLine();

    ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);

    // Extract text (OCR) from a URL image using the Read API
    // ReadFileUrl(client, READ_TEXT_URL_IMAGE).Wait();

    var fileName = @"D:\POC\computer-vision-quickstart\Set1-B.jpg";

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

        // Display the found text.
        Console.WriteLine();
        var textUrlFileResults = results.AnalyzeResult.ReadResults;
        foreach (ReadResult page in textUrlFileResults)
        {
            foreach (Line line in page.Lines)
            {
                Console.WriteLine(line.Text);
            }
        }
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

        // <snippet_extract_display>
        // Display the found text.
        Console.WriteLine();
        var textUrlFileResults = results.AnalyzeResult.ReadResults;
        foreach (ReadResult page in textUrlFileResults)
        {
            foreach (Line line in page.Lines)
            {
                Console.WriteLine(line.Text);
            }
        }
        Console.WriteLine();
    }

