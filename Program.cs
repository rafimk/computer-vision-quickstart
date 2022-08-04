using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Text;
using System.Text.RegularExpressions;
using computer_vision_quickstart;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;


// Add your Computer Vision subscription key and endpoint
string subscriptionKey = "d4f537561bdd405489046ac0e633cdc0";
    string endpoint = "https://uaekmcc.cognitiveservices.azure.com/";

    const string READ_TEXT_URL_IMAGE = @"https://kmccfileupload.blob.core.windows.net/images/Set2-A.jpg";


    Console.WriteLine("Azure Cognitive Services Computer Vision - .NET quickstart example");
    Console.WriteLine();

    ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);

    // Extract text (OCR) from a URL image using the Read API
    // ReadFileUrl(client, READ_TEXT_URL_IMAGE).Wait();

    var fileName = @"D:\POC\computer-vision-quickstart\Set2-A.jpg";

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

        var localFilePath = "Set2-A.jpg";

        string connectionString = "DefaultEndpointsProtocol=https;AccountName=kmccfileupload;AccountKey=h+PFVsQg8A6A9S43/lDDABLNO6GyzGTmGRgH6op5KHwXo85jyBrD7XivcCtWvZvZJHNFIp84my43+AStWH/JCw==;EndpointSuffix=core.windows.net";
        string containerName = "images";
            
        // Create a BlobServiceClient object which will be used to create a container client
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        BlobClient blobClient = containerClient.GetBlobClient(localFilePath);


        using var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream);
        memoryStream.Position = 0;

        // Read text from URL ** Enable for local **
        var textHeaders = await client.ReadInStreamAsync(memoryStream);
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
            var genderIndexStart = finalResult.IndexOf("Sex:");
            var gender = finalResult.Substring(genderIndexStart + 5, 1);
            Console.WriteLine("New card front side.");
            Console.WriteLine($"Gender : {gender}");
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
            var genderIndexStart = finalResult.IndexOf("Sex:");
            var gender = finalResult.Substring(genderIndexStart + 5, 1);
            Console.WriteLine("Old card back side");
            Console.WriteLine($"Gender : {gender}");
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
   
