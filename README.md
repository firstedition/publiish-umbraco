# Publii
To run and debug the web application locally, you first need to add the following to your Program.cs:

	using Publii.Umbraco.Extensions;

	...    

	WebApplication app = builder.Build();
    
    // using Publii
    app.UsePubliiRouting(app.Configuration);
    
    await app.BootUmbracoAsync();

After installation a new "Publii" section will be available to add to Umbraco user groups.
