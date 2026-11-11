# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TchOpenSource is a PDF utility web application by The Convergency Hub. It provides browser-based tools for PDF manipulation: merging/stitching, compression, HTML-to-PDF conversion, password protection, text extraction, signature/initial stamping, and QR code verification stamping.

## Architecture

Two projects in one solution:

- **TchOpenSource** — ASP.NET MVC 4.8 web app (IIS Express, port 44384 SSL). The main application.
  - `Controllers/PdfUtilsController` — All PDF operations (merge, compress, protect, make, extract, stamp, initial, signature, readonly)
  - `Controllers/HomeController` — Landing pages and QR verification endpoint (`/Home/Verify/{guid}`)
  - `Models/OpenSourceConfig` — Reads Azure Storage connection settings from `Web.config` AppSettings/ConnectionStrings
  - `Lib/ByteArrayPdfSplitter` — PDF splitting utility
  - Key dependencies: iText7 (PDF), iText7.pdfhtml (HTML-to-PDF), QRCoder, SharpZipLib, Azure.Storage.Blobs, Convergency.AzureBlob (external project)

- **tchopensourcecli** — .NET Core 3.1 console app. REST client that calls the web app's API endpoints using RestSharp. Points to `https://localhost:44300/` by default. Has embedded resources (affidavit.html, preface.pdf, report.pdf).

## External Dependency

The solution references `Convergency.AzureBlob` project at `../Convergency Web/ConvergencyBloc/Convergency.AzureBlob/`. This handles Azure Blob Storage uploads for the stamped PDF verification feature.

## Build

Open `TchOpenSource.sln` in Visual Studio. NuGet packages are in the `packages/` directory (packages.config style). Build configurations: Debug, Release, GridUK, Inani, Staging.

The CLI project has its own solution at `tchopensourcecli/TchOpenSourceClientOnly/TchOpenSourceClientOnly.sln` and can be built independently with `dotnet build` from the `tchopensourcecli/` directory.

## Configuration

Azure Storage settings are required in `Web.config`:
- AppSetting: `CloudStorageContainerReference`
- ConnectionString: `StorageConnectionString`

## Key Patterns

- Multiple file uploads return a ZIP archive; single file uploads return the PDF directly
- PDF operations use iText7's `PdfReader`/`PdfWriter` with `MemoryStream` for in-memory processing
- The stamping feature generates a GUID, creates a QR code with verification metadata, uploads the stamped PDF to Azure Blob Storage, and embeds the QR on page 1
