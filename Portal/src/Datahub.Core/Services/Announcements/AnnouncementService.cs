using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.UserManagement;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Announcements;

public class AnnouncementService : IAnnouncementService
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
    private readonly CultureService _cultureService;

    public AnnouncementService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory, CultureService cultureService)
    {
        _dbContextFactory = dbContextFactory;
        _cultureService = cultureService;
    }

    public async Task<List<AnnouncementPreview>> GetActivePreviews()
    {
        List<AnnouncementPreview> mocks = new();

        mocks.Add(new(1, "Lorem ipsum dolor sit amet", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Nunc mattis enim ut tellus. Massa ultricies mi quis hendrerit dolor magna eget est lorem. Gravida arcu ac tortor dignissim convallis aenean et tortor. Arcu felis bibendum ut tristique. Sed lectus vestibulum mattis ullamcorper velit sed ullamcorper morbi. Dui nunc mattis enim ut tellus elementum sagittis vitae. Penatibus et magnis dis parturient montes nascetur ridiculus mus mauris."));
        mocks.Add(new(2, "Sit amet nisl suscipit adipiscing bibendum est ultricies", "Sit amet nisl suscipit adipiscing bibendum est ultricies. Urna nunc id cursus metus. Tortor at auctor urna nunc id. At quis risus sed vulputate. Ornare suspendisse sed nisi lacus sed viverra. Consequat nisl vel pretium lectus quam id. Nec nam aliquam sem et tortor consequat id porta nibh. Orci phasellus egestas tellus rutrum tellus pellentesque. Eget duis at tellus at urna condimentum mattis pellentesque id."));
        mocks.Add(new(3, "Tortor consequat id porta nibh venenatis cras sed", "Tortor consequat id porta nibh venenatis cras sed. Gravida quis blandit turpis cursus. Pharetra diam sit amet nisl suscipit. Arcu ac tortor dignissim convallis aenean et. Sit amet nulla facilisi morbi tempus iaculis urna id. Malesuada pellentesque elit eget gravida cum sociis. Turpis cursus in hac habitasse. Vulputate eu scelerisque felis imperdiet proin. Id interdum velit laoreet id donec."));
        mocks.Add(new(4, "Euismod quis viverra nibh cras pulvinar mattis", "Euismod quis viverra nibh cras pulvinar mattis nunc sed blandit. Egestas purus viverra accumsan in nisl nisi scelerisque eu ultrices. Ultrices eros in cursus turpis massa tincidunt dui ut. Sapien pellentesque habitant morbi tristique. Scelerisque fermentum dui faucibus in. Sit amet porttitor eget dolor morbi."));

        return await Task.FromResult(mocks);

        //await using var ctx = await _dbContextFactory.CreateDbContextAsync();

        //var isFrench = _cultureService.IsFrench;
        //var today = DateTime.Now.Date;

        //var articles = await ctx.Announcements
        //    .Where(e => today > e.StartDateTime && (!e.EndDateTime.HasValue || today < e.EndDateTime.Value))
        //    .Select(e => new AnnouncementPreview(e.Id, isFrench ? e.PreviewFr : e.PreviewEn))
        //    .ToListAsync();

        //return articles;
    }
}
