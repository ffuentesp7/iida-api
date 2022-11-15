using Iida.Shared.Models;

using Microsoft.EntityFrameworkCore;

namespace Iida.Api.Contexts;

public class AppDbContext : DbContext {
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
	public DbSet<Order>? Orders { get; set; }
	public DbSet<EvapotranspirationMap>? EvapotranspirationMaps { get; set; }
	public DbSet<MeteorologicalData>? MeteorologicalDatas { get; set; }
	public DbSet<SatelliteImage>? SatelliteImages { get; set; }
}