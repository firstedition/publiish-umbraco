using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Publii.Umbraco.Models;

[TableName(Tables.Publications)]
[PrimaryKey("Id", AutoIncrement = true)]
[ExplicitColumns]
public class Publication
{
	[PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
	[Column("Id")]
	public int Id { get; set; }

	[Column("Guid")]
	public Guid Guid { get; set; }
    
	[Column("Name")]
	[NullSetting]
	public string? Name { get; set; }
    
	[Column("Created")]
	[NullSetting]
	public DateTime? Created { get; set; }
    
	[Column("Updated")]
	[NullSetting]
	public DateTime? Updated { get; set; }
    
	[Column("Description")]
	[Length(2000)]
	[NullSetting]
	public string? Description { get; set; }
    
	[Column("UrlSegment")]
	[NullSetting]
	public string? UrlSegment { get; set; }
    
	[Column("MediaRootId")]
	[NullSetting]
	public int? MediaRootId { get; set; }
    
	[Column("Processed")]
	public bool Processed { get; set; }
	
	[Ignore]
	public string? BaseUrl { get; set; }
}