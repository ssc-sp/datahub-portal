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
        public void GivenStrArray_ThenReadToClass()
        {
            string[] values = { "item1", "item2", "item3", "item4" };
            Assert.True(values.Count() == 4);

            var properties = typeof(Importlist).GetProperties();

            //Check to make sure array length will fit into the object
            Assert.True(values.Count() == properties.Length);

            Importlist list = new Importlist();

            int index = 0;
            foreach (var item in properties)
            {
                //check to make sure the types in the array match the object
                //in prod this has to be a bit more subtle
                var arrayValue = values[index];

                Assert.True(arrayValue.GetType() == item.PropertyType);
                var propertyName = item.Name;
                var objectproperty = list.GetType().GetProperty(propertyName);
                objectproperty.SetValue(list, values[index]);
                index++;
            }

            Assert.True(list.item1 == "item1");
            Assert.True(list.item2 == "item2");
            Assert.True(list.item3 == "item3");
            Assert.True(list.item4 == "item4");

            Assert.True(properties.Length == 4);
        }


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

        //[Fact]
        //public void TestListCategoriesForPIPTombstone()
        //{
        //    var s1 = typeof(PIP_Tombstone).GetAeModelFormCategories();
        //    Assert.Equal(7, s1.Count);
        //    //Assert.Collection(s1, e => Assert.Null(e.category), e => Assert.Equal("Identification", e.category), e => Assert.Equal("Details", e.category));
        //    //var (c1, l1) = s1[0];
        //}

        
    }

    public class Importlist
    { 
        public string item1 { get; set; }
        public string item2 { get; set; }
        public string item3 { get; set; }
        public string item4 { get; set; }
    }
}
