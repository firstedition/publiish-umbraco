
# Publiish / Publii for Umbraco
To run and debug the web application locally, you first need to add the following to your Program.cs:

	using Publii.Umbraco.Extensions;

	...    

	WebApplication app = builder.Build();
    
    // using Publii
    app.UsePubliiRouting(app.Configuration);
    
    await app.BootUmbracoAsync();

Default url routing is `/publications/your-publication`

To change the Root Url Segment, add the following config to your appsettings.json:

	"Publii": {
    	"RootUrlSegment": "rapporter"
  	}

This will change your url routing to `/rapporter/your-publication`

---   

After installation a new "Publii" section will be available to add to Umbraco user groups.
