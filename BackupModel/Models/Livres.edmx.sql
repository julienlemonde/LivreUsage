
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 09/27/2015 13:15:10
-- Generated from EDMX file: C:\Users\julien\Documents\GitHub\LivreUsage\WebApplication2\WebApplication2\Models\Livres.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [aspnet-WebApplication2-20150917084726];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Coop]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Coop];
GO
IF OBJECT_ID(N'[dbo].[LivreInventaire]', 'U') IS NOT NULL
    DROP TABLE [dbo].[LivreInventaire];
GO
IF OBJECT_ID(N'[dbo].[Livres]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Livres];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Coop'
CREATE TABLE [dbo].[Coop] (
    [Id] int  NOT NULL,
    [Nom] nvarchar(50)  NOT NULL,
    [Adresse] nvarchar(50)  NULL,
    [NomGestionnaire] nvarchar(50)  NULL
);
GO

-- Creating table 'LivreInventaire'
CREATE TABLE [dbo].[LivreInventaire] (
    [Id] int  NOT NULL,
    [CodeIdentification] nvarchar(50)  NOT NULL,
    [Etat] nvarchar(50)  NULL,
    [Quantite] int  NOT NULL,
    [Cooperative] int  NULL
);
GO

-- Creating table 'Livres'
CREATE TABLE [dbo].[Livres] (
    [Id] int  NULL,
    [Nom] nvarchar(50)  NULL,
    [Auteur] nvarchar(50)  NULL,
    [NbrPages] nvarchar(50)  NULL,
    [Prix] decimal(5,2)  NULL,
    [IdCoop] int  NULL,
    [CodeIdentification] nvarchar(50)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Nom] in table 'Coop'
ALTER TABLE [dbo].[Coop]
ADD CONSTRAINT [PK_Coop]
    PRIMARY KEY CLUSTERED ([Nom] ASC);
GO

-- Creating primary key on [Id] in table 'LivreInventaire'
ALTER TABLE [dbo].[LivreInventaire]
ADD CONSTRAINT [PK_LivreInventaire]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [CodeIdentification] in table 'Livres'
ALTER TABLE [dbo].[Livres]
ADD CONSTRAINT [PK_Livres]
    PRIMARY KEY CLUSTERED ([CodeIdentification] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------