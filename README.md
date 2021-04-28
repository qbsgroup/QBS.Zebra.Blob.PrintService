# QBS.Zebra.Blob.PrintService
Print Service to stream ZPL from Azure Blob to Zebra Printers

# Cloud Printing
Printing in cloud scenario's can be a bit challenging, especially if the amount of prints are relatively high and/or the printer is not modern (born in the cloud).

Examples used here is a zebra printer using ZPL streams but other examples could be printing a CMR (freight document) using matrix printers.

These printers are often a bit older and can be quite expensive to replace.

Printing should not be blocking any cloud migration so in order to connect older devices to Business Central we decided to share this repository.

## What does it do
Business Central allows you to store files in Azure Blob Storage with a simple http request. These files can contain information required for printing such as a zpl string or a PDF file.

This tool allows you to run a windows service on any machine that has a connection to your local printers. Most often this will be an Azure Virtual Machine connected to the local network, but in theory it can also be an old box sitting in a corner of your office.

Periodically the windows service will connect to the Azure Blob Storage and check for new files. The contents of this file can be processed and sent to any printer using software that is supported using DotNET Framework.

In the example, a ZPL string is sent to a Zebra printer using an open socket connection.

Optionally you can use any publically available libraries to send contents of PDF files to printers in your network.

An example tool can be pdfprint.exe from VeryPDF.com. This can be executed from the MSDos command prompt. Another option is to add SpirePDF to the project from nuget.org.

## How to install?
1. Build solution
2. Browse to build directory (debug: *{repo}\AzureBlobService\bin\debug* | release: *{repo}\AzureBlobService\bin\release*)
3. AzureBlobService.exe *--install*

## How to uninstall?
1. Build solution
2. Browse to build directory (debug: {repo}\AzureBlobService\bin\debug | release: {repo}\AzureBlobService\bin\release)
3. AzureBlobService.exe *--uninstall*
