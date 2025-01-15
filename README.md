# Middleware for Filtering and Masking API Requests in .NET Core
### Overview 

This project demonstrates how to implement middleware in ASP.NET Core to filter and mask sensitive fields in API requests and responses. The middleware enhances API security, ensures compliance with data privacy regulations, and provides a foundation for scalable and robust API architectures.

### Features

* Checks sensitive information like `password`, `ssn`, and `creditCard`.
* Let you choose what to do: hide fields (like `*****`) or take fields out completely.
* Works with complex and nested JSON data, including lists and objects.
* Easy to use again, simple to add to any **ASP.NET Core** project.

## Table of Contents
1. [Installation](#installation)
2. [Middleware Design and Workflow](#middleware-design-and-workflow)
3. [Key Methods](#key-methods)
4. [Adding Middleware to your Project](#adding-middleware-to-your-project)
5. [Examples](#examples)
5. [Customization](#customization)
6. [Contributing](#contributing)
7. [License](#license)

## Installation

### Prerequisites
- **.NET Core 6 or later** installed.
- Visual Studio Code.
- Postman for testing API requests.

### Steps
1. Clone this repository:
   ```
   git clone https://github.com/frttnk/Middleware-Request-Filter-Mask.git
   cd your-repository
2. Open the project in Visual Studio Code
3. Restore dependencies and build the solution

   ```
   dotnet restore
   dotnet build

## Middleware Design and Workflow

The middleware checks incoming `POST` requests:
1. Takes the request body and reads it as JSON. 
2. Looks through the JSON data to find private information.
3. Based on how it is set up:
   - It hides private fields (like changing values to *****).
   - Takes out private fields completely.
4. Puts the changed request body in place of the old one.
5. Sends the changed request to the next middleware or controller in line.

## Key Methods 

- `ProcessJsonElement`: Looks through and changes JSON parts (objects, lists, text) one by one, going deeper when needed.
- `InvokeAsync`: The primary starting point for middleware, it manages to catch and handle requests.

## Adding Middleware to Your Project
1. Add the middleware to the request pipeline in the `Program.cs`

```C#
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Add middleware to the pipeline
app.UseMiddleware<RequestFilteringMiddleware>(true, false);

app.MapControllers();
app.Run();
```

- `true`: Enables masking of sensitive fields.
- `false`: Disables removal sensitive fields.

2. Create a test controller for validation

```C#
[ApiController]
[Route("api/[controller]")]
public class MiddlewareController : ControllerBase
{
    [HttpPost]
    public IActionResult ReturnResponse([FromBody] Dictionary<string, object> requestData)
    {
        return Ok(new
        {
            Data = requestData
        });
    }
}
```
## Examples

### Request 

```JSON
{
    "data": {
        "user": {
            "name": {
                "first": "Firat",
                "last": "Tonak"
            },
            "email": "firattonak.com@firattonak.com",
            "ssn": "123-45-6789"
        },
        "order": {
            "paymentMethod": {
                "card": {
                    "accountnumber": "1111 222 3333 1111"
                }
            }
        }
    }
}
```

### Response with Masking

```JSON
{
    "data": {
        "data": {
            "user": {
                "name": {
                    "first": "Firat",
                    "last": "Tonak"
                },
                "email": "firattonak.com@firattonak.com",
                "ssn": "*****"
            },
            "order": {
                "paymentMethod": {
                    "card": {
                        "accountnumber": "*****"
                    }
                }
            }
        }
    }
}
```

### Response with Removal

```JSON
{
    "data": {
        "data": {
            "user": {
                "name": {
                    "first": "Firat",
                    "last": "Tonak"
                },
                "email": "firattonak.com@firattonak.com"
            },
            "order": {
                "paymentMethod": {
                    "card": {}
                }
            }
        }
    }
}
```

## Customization 

### Modify Sensitive Keywords

Update the SensitiveData class to add new sensitive words:

```C#
public static readonly List<string> Keywords = new()
{ 
    "password", 
    "ssn", 
    "creditCard", 
    "accountnumber"
};
```

### Switch Between Masking and Removal

```C#
app.UseMiddleware<RequestFilteringMiddleware>(false, true);
```

## Contributing

Welcome. Help make this project better! Here is how you can help:

1. Fork the repository
2. Create a new branch for your feature or bugfix
```
git checkout -b feature-name
```
3. Commit your changes and push to your fork
4. Submit a pull request with a detailed description of your changes.

### License

This project is licensed under the [MIT License](./LICENSE). You are free to use, modify, and distribute it as needed.
