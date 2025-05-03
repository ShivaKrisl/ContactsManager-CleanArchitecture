using Microsoft.AspNetCore.Mvc.Filters;
using Service_Contracts;
using ContactManagement.Controllers;
using Entities_DTO;
using Service_Classes;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ContactsManager.UI.Filters.ActionFilters
{
    public class PersonCreateErrorActionFilter : IAsyncActionFilter
    {

        private readonly ICountriesGetterService _countriesGetterService;

        public PersonCreateErrorActionFilter(ICountriesGetterService countriesGetterService)
        {
            _countriesGetterService = countriesGetterService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            // executes before action method

            if(context.Controller is PersonsController personsController)
            {
                if (!personsController.ModelState.IsValid)
                {
                    List<CountryResponse>? countryResponses = await _countriesGetterService.GetAllCountries();
                    personsController.ViewBag.countries = countryResponses;

                    personsController.ViewBag.errors = personsController.ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage).ToList();

                    var personAddRequest = context.ActionArguments["personRequest"] as PersonRequest;

                    context.Result = personsController.View(personAddRequest); // return to the view with errors and short circuit the action method
                }
                else
                {
                    await next(); // invoke the next filter or action method

                }
            }
            else
            {
                await next(); // invoke the filter action method
            }
        }
    }
}
