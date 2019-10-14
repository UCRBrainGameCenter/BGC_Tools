using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using BGC.Reports;

namespace BGC.Tests
{
    public class CreateTestReports
    {
        [Test]
        public void CreateTests()
        {
            {
                ReportElement reportElement = new ReportElement(
                    "SUBJ001",
                    "Modulation Battery",
                    new System.DateTime(2018, 10, 1, 12, 1, 0));

                reportElement.AddData("SM", "3.0dB");
                reportElement.AddData("TM", "3.5dB");
                reportElement.AddData("STM", "1.1dB");

                reportElement.SaveIfNecessary();
            }

            {
                ReportElement reportElement = new ReportElement(
                    "SUBJ001",
                    "Modulation Battery",
                    new System.DateTime(2018, 11, 12, 15, 1, 0));

                reportElement.AddData("SM", "2.0dB");
                reportElement.AddData("TM", "3.8dB");
                reportElement.AddData("STM", "1.2dB");

                reportElement.SaveIfNecessary();
            }

            {
                ReportElement reportElement = new ReportElement(
                    "SUBJ001",
                    "Noise Battery",
                    new System.DateTime(2018, 11, 12, 15, 42, 12));

                reportElement.AddData("Noise", "2.0dB");
                reportElement.AddData("Passed", "True");

                reportElement.SaveIfNecessary();
            }

            {
                ReportElement reportElement = new ReportElement(
                    "SUBJ001",
                    "Modulation Battery",
                    new System.DateTime(2019, 3, 12, 15, 1, 0));

                reportElement.AddData("SM", "1.8dB");
                reportElement.AddData("TM", "3.2dB");
                reportElement.AddData("STM", "0.9dB");

                reportElement.SaveIfNecessary();
            }

            {
                ReportElement reportElement = new ReportElement(
                    "SUBJ001",
                    "Noise Battery",
                    new System.DateTime(2019, 3, 13, 14, 45, 0));

                reportElement.AddData("Passed", "False");

                reportElement.SaveIfNecessary();
            }

            {
                ReportElement reportElement = new ReportElement(
                    "SUBJ002",
                    "Modulation Battery",
                    new System.DateTime(2018, 10, 2, 12, 15, 0));

                reportElement.AddData("SM", "3.2dB");
                reportElement.AddData("TM", "2.5dB");
                reportElement.AddData("STM", "1.1dB");

                reportElement.SaveIfNecessary();
            }

            {
                ReportElement reportElement = new ReportElement(
                    "SUBJ003",
                    "Modulation Battery",
                    new System.DateTime(2018, 10, 3, 11, 15, 0));

                reportElement.AddData("SM", "2.2dB");
                reportElement.AddData("TM", "2.67dB");
                reportElement.AddData("STM", "0.02dB");

                reportElement.SaveIfNecessary();
            }

            {
                ReportElement reportElement = new ReportElement(
                    "SUBJ004",
                    "Modulation Battery",
                    new System.DateTime(2019, 1, 2, 11, 45, 0));

                reportElement.AddData("SM", "1.2dB");
                reportElement.AddData("STM", "0.20dB");

                reportElement.SaveIfNecessary();
            }
        }
    }
}
