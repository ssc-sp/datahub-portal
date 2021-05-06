using Elemental.Code;
using Microsoft.Graph;
using NRCan.Datahub.ProjectForms.Data.PIP;
using NRCan.Datahub.Portal.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Sdk;
using NRCan.Datahub.Portal.Pages.Forms.Datahub;

namespace FormTests
{
    public class UnitTest1
    {


        [Fact]
        public void TestShortenContactList()
        {
            var l1 = "john doe <john.doe@canada.ca>; mark henri <mark.henri@canada.ca>;";
            Assert.Equal("john doe, mark henri", Projects.ShortenContactList(l1));
        }



        [Fact]
        public void SplitEmailAddresses()
        {
            //var input = "Newman, Christine (NRCAN/RNCAN) <christine.newman@canada.ca>; Shelat, Yask (NRCAN/RNCAN) <yask.shelat@canada.ca>; Liu, Diana (NRCAN/RNCAN) <diana.liu@canada.ca>";
            //var blocks = input.Split(';', StringSplitOptions.RemoveEmptyEntries);
            //var valid = blocks.Select(e => ExtractEmail(e)).Where(e => e != null).ToList();
            //Assert.Collection(valid, e => e.Equals("christine.newman@canada.ca"), e => e.Equals("yask.shelat@canada.ca"), e => e.Equals("diana.liu@canada.ca"));
        }

        [Fact]
        public void TestShortenString()
        {
            var longString = "The program '[40312] NRCan.Datahub.ProjectForms.exe: Program Trace' has exited with code 0 (0x0).";
            Assert.Equal("The program '[40312]...", Projects.ShortenString(longString, 20));
        }

        [Fact]
        public void TestListCategoriesForPIPTombstone()
        {
            var s1 = typeof(PIP_Tombstone).GetAeModelFormCategories();
            Assert.Equal(7, s1.Count);
            //Assert.Collection(s1, e => Assert.Null(e.category), e => Assert.Equal("Identification", e.category), e => Assert.Equal("Details", e.category));
            //var (c1, l1) = s1[0];
        }

    }
}
