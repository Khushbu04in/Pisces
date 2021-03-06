﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Pisces.Domain.Models.Alleles;
using Pisces.Domain.Types;

namespace TestUtilities
{
    public class TestVariantsLoader
    {
        public static List<CalledAllele> LoadBaseCalledAllelesFile(string filepath)
        {
            var variants = new List<CalledAllele>();
            var columns = new string[0];

            using (var reader = new StreamReader(File.OpenRead(filepath)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var tokens = line.Split('\t');

                    if (line.StartsWith("Chromosome"))
                        columns = tokens;
                    else
                    {
                        var variant = new CalledAllele(AlleleCategory.Snv); // note doesn't matter what the call type is, vcf writer doesnt care
                        for(var i = 0; i < columns.Length; i ++)
                        {
                            var column = columns[i];
                            var dataValue = tokens[i];

                                var type = typeof(CalledAllele);
                                var property = type.GetProperty(column);

                            switch (column)
                            {
                                case "Chromosome":
                                case "Reference":
                                case "Alternate":
                                    property.SetValue(variant, dataValue);
                                    break;
                                case "Coordinate":
                                case "Qscore":
                                case "TotalCoverage":
                                case "AlleleSupport":
                                    property.SetValue(variant, Int32.Parse(dataValue));
                                    break;
                                case "FractionNoCalls":
                                    property.SetValue(variant, float.Parse(dataValue));
                                    break;
                                case "StrandBiasScore":
                                    variant.StrandBiasResults.GATKBiasScore = float.Parse(dataValue);
                                    break;
                                case "Filters":
                                    var filterStrings = dataValue.Split(',');
                                    foreach (var filter in filterStrings)
                                    {
                                        if (!string.IsNullOrEmpty(filter))
                                        {
                                            var filterEnum = (FilterType) Enum.Parse(typeof (FilterType), filter, true);
                                            variant.Filters.Add(filterEnum);
                                        }
                                    }
                                    break;
                                case "Genotype":
                                    variant.Genotype = (Genotype) Enum.Parse(typeof (Genotype), dataValue, true);
                                    break;
                            }
                        }

                        if (variant.Genotype == Genotype.HomozygousRef || variant.Genotype == Genotype.RefLikeNoCall)
                        {
                            variants.Add(Map(variant));
                        }
                        else
                            variants.Add(variant);
                    }
                }
            }

            return variants;
        }

        public static List<CalledAllele> LoadBaseCalledAllelesArray(string[] candidates)
        {
            var variants = new List<CalledAllele>();
            var columns = new string[0];
            foreach (var line in candidates) {
            {
                var tokens = line.Split('\t');

                if (line.StartsWith("Chromosome"))
                    columns = tokens;
                else
                {
                    var variant = new CalledAllele(AlleleCategory.Snv); // note doesn't matter what the call type is, vcf writer doesnt care
                    for (var i = 0; i < columns.Length; i++)
                    {
                        var column = columns[i];
                        var dataValue = tokens[i];

                        var type = typeof(CalledAllele);
                        var property = type.GetProperty(column);

                        switch (column)
                        {
                            case "Chromosome":
                            case "Reference":
                            case "Alternate":
                                property.SetValue(variant, dataValue);
                                break;
                            case "Coordinate":
                            case "Qscore":
                            case "TotalCoverage":
                            case "AlleleSupport":
                                property.SetValue(variant, Int32.Parse(dataValue));
                                break;
                            case "FractionNoCalls":
                                property.SetValue(variant, float.Parse(dataValue));
                                break;
                            case "StrandBiasScore":
                                variant.StrandBiasResults.GATKBiasScore = float.Parse(dataValue);
                                break;
                            case "Filters":
                                var filterStrings = dataValue.Split(',');
                                foreach (var filter in filterStrings)
                                {
                                    if (!string.IsNullOrEmpty(filter))
                                    {
                                        var filterEnum = (FilterType)Enum.Parse(typeof(FilterType), filter, true);
                                        variant.Filters.Add(filterEnum);
                                    }
                                }
                                break;
                            case "Genotype":
                                variant.Genotype = (Genotype)Enum.Parse(typeof(Genotype), dataValue, true);
                                break;
                        }
                    }

                    if (variant.Genotype == Genotype.HomozygousRef || variant.Genotype == Genotype.RefLikeNoCall)
                    {
                        variants.Add(Map(variant));
                    }
                    else
                        variants.Add(variant);
                }
            }
        }

            return variants;
        }


        private static CalledAllele Map(CalledAllele variant)
        {
            return new CalledAllele()
            {
                Chromosome = variant.Chromosome,
                ReferencePosition = variant.ReferencePosition,
                AlternateAllele = variant.AlternateAllele,
                ReferenceAllele = variant.ReferenceAllele,
                StrandBiasResults = variant.StrandBiasResults,
                Filters = variant.Filters,
                Genotype = variant.Genotype,
                AlleleSupport = variant.AlleleSupport,
                NumNoCalls = variant.NumNoCalls,
                VariantQscore = variant.VariantQscore,
                TotalCoverage = variant.TotalCoverage
            };
        }
    }
}
