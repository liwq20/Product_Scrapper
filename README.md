# Product Scraper

## Overview

This C# project offers a straightforward tool for scraping data from Newegg. Users can gather information such as product titles, prices, ratings, and URLs by setting specific search and sorting parameters. The scraped data is stored in an SQLite database, where users can perform various database operations like fetching, inserting, and deleting records. Additionally, the tool supports exporting data to CSV files for further analysis.

## Features

- **Data Scraping:** Collect data on products from Newegg using customizable search filters.
- **SQLite Database Integration:** Store and manage scraped data locally.
- **Database Management:** Perform operations like insert, fetch, and delete on the database.
- **Data Export:** Export data to CSV format for external use.
- **Customization:** Modify search and sorting settings to target specific data needs.

## Prerequisites

Before installation, make sure you have:

- .NET SDK (Version 5.0 or higher recommended)
- A suitable IDE, such as Visual Studio or VSCode

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/liwq20/Prodcut_Scrapper.git
   ```
2. Navigate to the project directory:
   ```bash
   cd Product_Scrapper
   ```
3. Install necessary dependencies:
   ```bash
   dotnet add package HtmlAgilityPack
   dotnet add package HtmlAgilityPack.CssSelectors
   dotnet add package System.Data.SQLite
   ```

## Usage

To use the scraper, follow these steps:

1. Open your terminal.
2. Build the project:
   ```bash
   dotnet build
   ```
3. Run the scraper:
   ```bash
   dotnet run
   ```
The scraper will execute based on default settings defined in the Main method. Adjust these parameters within the code to tailor the scraping process to your needs.

---

This rewritten README.md simplifies the original content while maintaining the essential details, making it accessible for users needing quick setup and usage instructions.
