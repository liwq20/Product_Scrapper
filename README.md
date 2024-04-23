
# Newegg Product Scraper

## Overview

This project is a simple scraper for Newegg, built in C#. It allows users to scrape product information such as title, price, ratings, and product URLs based on specified search criteria and sorting methods. This data is then stored in an SQLite database, which can be manipulated and queried through the application.

## Features

- Scrape product data from Newegg based on search criteria.
- Store scraped data in a local SQLite database.
- Fetch, insert, and delete operations on the database.
- Export scraped data to CSV.
- Customizable search and sorting parameters.

## Prerequisites

Before you start, ensure you have the following installed:
- .NET SDK (preferably version 5.0 or later)
- An IDE such as Visual Studio or VSCode

## Installation

1. Clone the repository:
   ```sh
   git clone https://github.com/your-username/newegg-scraper.git
   ```
2. Navigate to the project directory:
   ```sh
   cd newegg-scraper
   ```
3. Install dependencies:
   ```sh
   dotnet add package HtmlAgilityPack
   dotnet add package HtmlAgilityPack.CssSelectors
   dotnet add package System.Data.SQLite
   ```

## Usage

1. Open your terminal.
2. Build the project:
   ```sh
   dotnet build
   ```
3. Run the scraper:
   ```sh
   dotnet run
   ```

This will start the scraping process based on the default parameters set in the `Main` method. You can modify these parameters in the source code to customize the scraping process.

## Configuration

The scraper's behavior can be customized by modifying the `Scraper` class constructor parameters:

- `debug`: Set to `true` to enable detailed debug output.

You can also change the search phrase and sorting method directly in the `Main` method.

## Contributing

Contributions to this project are welcome! Please fork the repository and submit a pull request with your features or fixes.

