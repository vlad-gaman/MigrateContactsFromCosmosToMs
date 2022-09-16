﻿global using AutoFixture;
global using EFCore.BulkExtensions;
global using Microsoft.Azure.Cosmos;
global using Microsoft.Azure.Cosmos.Fluent;
global using Microsoft.Azure.Cosmos.Linq;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using MigrateContactsFromCosmosToMs;
global using MigrateContactsFromCosmosToMs.Cosmos;
global using MigrateContactsFromCosmosToMs.Cosmos.Models;
global using MigrateContactsFromCosmosToMs.MsSql;
global using MigrateContactsFromCosmosToMs.MsSql.Models;
global using MigrateContactsFromCosmosToMs.Services;
global using Newtonsoft.Json;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.Diagnostics;