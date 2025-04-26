using Microsoft.AspNetCore.Mvc.Filters;
using Entities_DTO;
using ContactManagement.Controllers;
using Enums;
namespace Filters.ActionFilters
{
    public class PersonListActionFilter : IActionFilter
    {
        private readonly ILogger<PersonListActionFilter> _logger;

        public PersonListActionFilter(ILogger<PersonListActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // executes before action method

            // adding action args so that can be accessed in onActionExecuted method
            context.HttpContext.Items["actionArgs"] = context.ActionArguments;

            // Todo : Log before getting persons list 
            _logger.LogInformation("-------- {FilterName}.{MethodName} called --------", nameof(PersonListActionFilter), nameof(OnActionExecuting));

            // see action method parameters -- here we get as dictionary type
            if (context.ActionArguments.ContainsKey("searchBy"))
            {
                string? searchBy = context.ActionArguments["searchBy"] as string;
                if (!string.IsNullOrEmpty(searchBy))
                {
                    var searchOptions = new List<string>()
                    {
                        nameof(PersonResponse.FirstName),
                        nameof(PersonResponse.LastName),
                        nameof(PersonResponse.Email),
                        nameof(PersonResponse.PhoneNumber)
                    };

                    if (searchOptions.Any(temp => temp == searchBy))
                    {
                        _logger.LogInformation($"SearchBy: {searchBy}");
                        // you can change the value of searchBy here if needed
                        // context.ActionArguments["searchBy"] = SOME VALUE;

                    }
                    else
                    {
                        _logger.LogWarning($"Invalid SearchBy value: {searchBy} setting to FirstName");
                        // you can set a default value or throw an exception
                        context.ActionArguments["searchBy"] = nameof(PersonResponse.FirstName);
                    }
                }


            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // executes after action method  

            _logger.LogInformation("-------- {FilterName}.{MethodName} called --------", nameof(PersonListActionFilter), nameof(OnActionExecuted));

            PersonsController personsController = (PersonsController)context.Controller;
            IDictionary<string, object?>? actionArgs = (IDictionary<string, object?>)context.HttpContext.Items["actionArgs"];

            if (actionArgs != null)
            {
                if (actionArgs.ContainsKey("searchBy"))
                {
                    personsController.ViewData["currentSearchBy"] = Convert.ToString(actionArgs["searchBy"]);
                }
                if (actionArgs.ContainsKey("searchString"))
                {
                    personsController.ViewData["currentSearchString"] = Convert.ToString(actionArgs["searchString"]);
                }
                if (actionArgs.ContainsKey("sortBy"))
                {
                    personsController.ViewData["currentSortBy"] = Convert.ToString(actionArgs["sortBy"]);
                }
                if (actionArgs.ContainsKey("sortOrder"))
                {
                    if (Enum.TryParse(typeof(SortOrderEnum), Convert.ToString(actionArgs["sortOrder"]), out var sortOrder))
                    {
                        personsController.ViewData["currentSortOrder"] = (SortOrderEnum)sortOrder;
                    }
                    else
                    {
                        personsController.ViewData["currentSortOrder"] = SortOrderEnum.ASCENDING;
                    }
                }

                personsController.ViewBag.searchFields = new Dictionary<string, string>()
                {
                        { nameof(PersonResponse.FirstName), "First Name" },
                        {nameof(PersonResponse.LastName), "Last Name" },
                        {nameof(PersonResponse.Email), "Email Id" },
                        {nameof(PersonResponse.PhoneNumber), "Mobile Number" }
                };

            }

        }
    }
}
