using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackingApp;

namespace ProjectTesting
{
    [TestClass]
    public class GeolocationTest
    {

        [TestMethod]
        public void StartTrackingBtn_ClickedTest()
        {

            string t = "This is a test";

            bool expectedResult = false;
            bool actualResult = true;

            Assert.AreEqual(expectedResult, actualResult);

        }
    }
}
