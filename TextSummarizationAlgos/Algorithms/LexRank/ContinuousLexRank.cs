﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextSummarizationAlgos.DocumentProcessing;
using TextSummarizationAlgos.Utils;
using Utils;
using System.Collections;

namespace TextSummarizationAlgos.Algorithms.LexRank
{
    public class ContinuousLexRank : MultipleDocTextSummarizationAlgorithm
    {
        private double threshold = 0.1;
        private double dampingFactor = 0.15;

        public ContinuousLexRank(double threshold, double dampingFactor)
        {
            this.threshold = threshold;
            this.dampingFactor = dampingFactor;
        }

        //override public string generateSummary(DocsStatistics docStats, Document newDoc)
        override public string generateSummary(ArrayList docs, double compressionRatio)
        {
            string genSummary = "";

            ArrayList allSents = new ArrayList();

            foreach (Document doc in docs)
            {
                allSents.AddRange(doc.sentences);
            }

            double[][] idfModifiedCosineMatrix = LexRankCommon.generateIdfModifiedCosineMatrix(IDF.getInstance(), allSents);

            Trace.write(" IDF Cosine Matrix : ");
            Trace.write(MatrixUtil.printMatrix(idfModifiedCosineMatrix));

            double[] weightSums = new double[allSents.Count];

            for (int i = 0; i < weightSums.Length; i++)
            {
                weightSums[i] = 0;
            }

            double[][] idfModifiedCosineMatrixCopy = new double[allSents.Count][];

            for (int i = 0; i < idfModifiedCosineMatrixCopy.Length; i++)
            {
                idfModifiedCosineMatrixCopy[i] = new double[allSents.Count];
            }

            for (int i = 0; i < idfModifiedCosineMatrix.Length; i++)
            {
                for (int j = 0; j < idfModifiedCosineMatrix[i].Length; j++)
                {
                    idfModifiedCosineMatrixCopy[i][j] = idfModifiedCosineMatrix[i][j];

                    weightSums[i] += idfModifiedCosineMatrix[i][j];

                    if (idfModifiedCosineMatrix[i][j] > this.threshold)
                    {
                        idfModifiedCosineMatrix[i][j] = 1;
                    }
                    else
                    {
                        idfModifiedCosineMatrix[i][j] = 0;
                    }
                }
            }

            Trace.write(MatrixUtil.printMatrix(idfModifiedCosineMatrix));

            for (int i = 0; i < idfModifiedCosineMatrix.Length; i++)
                for (int j = 0; j < idfModifiedCosineMatrix[i].Length; j++)
                {
                    idfModifiedCosineMatrix[i][j] = (idfModifiedCosineMatrixCopy[i][j] / weightSums[j]) * idfModifiedCosineMatrix[i][j];

                    idfModifiedCosineMatrix[i][j] = (dampingFactor / idfModifiedCosineMatrix.Length) + ((1 - dampingFactor) * idfModifiedCosineMatrix[i][j]);
                }

            //Trace.write(MatrixUtil.printMatrix(idfModifiedCosineMatrix));

            double[] weights = LexRankCommon.powerMethod(idfModifiedCosineMatrix, 0.1);

            //Trace.write(MatrixUtil.printMatrix(weights));

            for (int i = 0; i < allSents.Count; i++)
            {
                ((Sentence)allSents[i]).weight = weights[i];
            }

            Sentence[] sents = (Sentence[])allSents.ToArray(new Sentence().GetType());

            genSummary = SummaryUtil.SummarizeByCompressionRatio(sents, compressionRatio);
            /*
            Array.Sort(sents, new SentenceComparer());
            Array.Reverse(sents);

            foreach (Sentence sent in sents)
            {
                Trace.write(sent.fullText);
                Trace.write("Weight : " + sent.weight);
            }

            genSummary = getText(sents);
            //*/

            return (genSummary);
        }
    }
}
