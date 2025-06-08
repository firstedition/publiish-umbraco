using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Publii.Umbraco.Models;
using Publii.Umbraco.Providers.Interfaces;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;

namespace Publii.Umbraco.Controllers.Tree;

[Tree("publiiApp", "publiiTreeApp", TreeTitle = "Publications", TreeGroup = "publiiGroup", SortOrder = 1)]
[PluginController("Publii")]
public class PubliiTreeController(
	ILocalizedTextService localizedTextService,
	UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
	IEventAggregator eventAggregator,
	IMenuItemCollectionFactory menuItemCollectionFactory,
	IPublicationProvider publicationProvider)
	: TreeController(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
{
	protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
	{
		var nodes = new TreeNodeCollection();
        
		// check if we're rendering the root node's children
		if (id != Constants.System.Root.ToInvariantString())
			return nodes;
        
		// get publications from storage
		var publicationsStatus = publicationProvider.GetAll().GetAwaiter().GetResult();
		IEnumerable<Publication>? publications = null;
		if (publicationsStatus.IsSuccess)
			publications = publicationsStatus.Value;
        
		// get keys
		var publicationKeys = new Dictionary<int, string>();
		foreach (var publication in publications ?? new List<Publication>())
		{
			publicationKeys.Add(publication.Id, publication.Name ?? "-");
		}

		// loop through publication keys and create a tree item for each one
		foreach (var publicationKey in publicationKeys)
		{
			// add each node to the tree collection using the base CreateTreeNode method
			// it has several overloads, using here unique Id of tree item,
			// -1 is the Id of the parent node to create, eg the root of this tree is -1 by convention
			// - the querystring collection passed into this route
			// - the name of the tree node
			// - css class of icon to display for the node
			// - and whether the item has child nodes
			var node = CreateTreeNode(publicationKey.Key.ToString(), "-1", queryStrings, publicationKey.Value, "icon-book", false);
			nodes.Add(node);
		}

		return nodes;
	}
	
	protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
	{
		// create a Menu Item Collection to return so people can interact with the nodes in your tree
		var menu = menuItemCollectionFactory.Create();

		if (id == Constants.System.Root.ToInvariantString())
		{
			// root actions, perhaps users can create new items in this tree, or perhaps it's not a content tree, it might be a read only tree, or each node item might represent something entirely different...
			// add your menu item actions or custom ActionMenuItems
			menu.Items.Add(new CreateChildEntity(LocalizedTextService));
			// add refresh menu item (note no dialog)
			menu.Items.Add(new RefreshNode(LocalizedTextService, true));
		}
		else
		{
			// add a delete action to each individual item
			menu.Items.Add<ActionDelete>(LocalizedTextService, true, opensDialog: true);
		}

		return menu;
	}
}