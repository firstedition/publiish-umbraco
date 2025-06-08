using Microsoft.Extensions.Logging;
using Publii.Umbraco.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Publii.Umbraco.Migrations;

public class PublicationTableMigration(
	IMigrationPlanExecutor migrationPlanExecutor,
	ICoreScopeProvider coreScopeProvider,
	IKeyValueService keyValueService,
	IRuntimeState runtimeState)
	: INotificationHandler<UmbracoApplicationStartingNotification>
{
	public void Handle(UmbracoApplicationStartingNotification notification)
	{
		if (runtimeState.Level < RuntimeLevel.Run)
		{
			return;
		}

		// Create a migration plan for a specific project/feature
		// We can then track that latest migration state/step for this project/feature
		var migrationPlan = new MigrationPlan(Tables.Publications);

		// This is the steps we need to take
		// Each step in the migration adds a unique value
		migrationPlan.From(string.Empty)
			.To<AddPublicationTable>($"{Tables.Publications}-init");

		// Go and upgrade our site (Will check if it needs to do the work or not)
		// Based on the current/latest step
		var upgrader = new Upgrader(migrationPlan);
		upgrader.Execute(
			migrationPlanExecutor,
			coreScopeProvider,
			keyValueService);
	}

	private class AddPublicationTable(IMigrationContext context) : MigrationBase(context)
	{
		protected override void Migrate()
		{
			Logger.LogDebug("Running migration {MigrationStep}", "AddPublicationTable");

			// Lots of methods available in the MigrationBase class - discover with this.
			if (TableExists(Tables.Publications) == false)
			{
				Create.Table<Publication>().Do();
			}
			else
			{
				Logger.LogDebug("The database table {DbTable} already exists, skipping",
					Tables.Publications);
			}
		}
	}
}