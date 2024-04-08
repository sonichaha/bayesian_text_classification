using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayesApplication
{
    public class BayesianDocumentClassifier
    {
        private List<double> priorProbabilitiesList;
        private List<ConditionalWordProbability> conditionalWordProbabilityList;

        public BayesianDocumentClassifier()
        {
            priorProbabilitiesList = new List<double>();
            conditionalWordProbabilityList = new List<ConditionalWordProbability>();
        }

        // To do: Write this method
        // The method takes a document as input, then computes
        // the log probability as in Eq. (4.26) (in the compendium).
        //
        // Among other things, this involves summing (for each class label)
        // over the tokens in the document, ignoring any
        // token for which a conditional word probability is not available
        // (i.e. for those (rare) words that did not appear anywhere in the training set)
        //
        // Then infer (and return) the class label; again see Eq. (4.26).
        //
        // (Note (final edit!), the returned int label is not really used here. You must
        // also assign the inferred label to the document (done below, row 42). The
        // returned label WOULD be used if you extend your code to classify a new (single)
        // sentence ...
        public int Classify(Document document)
        {
            int numberOfClasses = priorProbabilitiesList.Count;
            int inferredClass = -1; // The inferred class label, either 0 or 1, should be assigned below
            PosteriorWordComparer posteriorWordComparer = new PosteriorWordComparer();
            // Add code here - See Eq. (4.26).
            document.TokenList.Sort();
            List<double> sumLogPosteriorList = new List<double>();
            for(int i = 0; i<numberOfClasses; ++i)
            {
                sumLogPosteriorList.Add(0.0);
            }
            foreach (string token in document.TokenList)
            {
                ConditionalWordProbability newItem = new ConditionalWordProbability();
                newItem.Word = token;
                int idx = conditionalWordProbabilityList.BinarySearch(newItem, posteriorWordComparer);

                for(int i = 0; i < numberOfClasses; ++i)
                {
                    if(idx >= 0)
                    {
                        sumLogPosteriorList[i] += Math.Log(conditionalWordProbabilityList[idx].ConditionalProbabilityList[i]);
                    }
                    else
                    {
                        sumLogPosteriorList[i] += 0;
                    }
                }
            }
            double maxProb = 0.0;
            for(int i = 0; i< numberOfClasses; ++i)
            {
                double probOfLabel = Math.Log(priorProbabilitiesList[i]) + sumLogPosteriorList[i];
                if(i == 0)
                {
                    maxProb = probOfLabel;
                    inferredClass = 0;
                }
                if(probOfLabel > maxProb)
                {
                    maxProb = probOfLabel;
                    inferredClass = i;
                }
            }
            
            document.InferredLabel = inferredClass; // Assign the inferred label here - needed later on, in the MainForm.
            return inferredClass;
        }

        // To do: Write this method.
        public void ComputeConditionalProbabilities(List<Document> documentList)
        {
            // Step 1: Find the list of all distinct words in the documents
            //         i.e. loop over the words, add the words to the wordList (defined just below)
            //         then reduce the list (Hint: Apply the Distinct() method)
            //         to make it a list of distinct words.
            List<string> wordList = new List<string>();
            
            // ... add code here
            foreach (Document doc in documentList)
            {
                foreach (string token in doc.TokenList)
                {
                    wordList.Add(token);
                }
            }
            wordList = wordList.Distinct().ToList();

            // Step 2: Define conditional probabilities (just the words for now)
            // This step is complete, you get it for free. :)
            conditionalWordProbabilityList = new List<ConditionalWordProbability>();
            foreach (string word in wordList)
            {
                ConditionalWordProbability cwp = new ConditionalWordProbability();
                cwp.Word = word;
                conditionalWordProbabilityList.Add(cwp);
            }

            // Step 3: Generate merged document
            // 
            int numberOfClasses = priorProbabilitiesList.Count;  // NOTE: See the MainForm, where the prior probabilities are defined.
            List<Document> mergedClassDocumentList = new List<Document>();

            // ... add code here: The mergedClassDocumentList should contain two
            //                    merged documents, one for each class (label).

            List<List<Document>> sameClassDocuments = new List<List<Document>>();
            for(int i = 0; i<numberOfClasses; ++i)
            {
                sameClassDocuments.Add(new List<Document>());
            }
            foreach (Document doc in documentList)
            {
                sameClassDocuments[doc.Label].Add(doc);
            }
            for (int i = 0; i < numberOfClasses; ++i)
            {
                mergedClassDocumentList.Add(Document.Merge(sameClassDocuments[i]));
            }

            // Now compute the conditional word probabilities:
            // NOTE: Use add-1 (Laplace) smoothing here! Very important!
            // See Eq. (4.25) in the compendium.
            int totalDistinctWordCount = conditionalWordProbabilityList.Count;

            // ... add code here: For each distinct token (word), run through
            //                    the merged documents (one per class), and
            //                    compute P(w_i|c_j), and store the values
            //                    in the conditionalWordProbabilityList.
            //                    See also the description in the ConditionalWordProbability class.

            foreach (ConditionalWordProbability cwp in conditionalWordProbabilityList)
            {
                string word = cwp.Word;
                for (int i = 0; i < numberOfClasses; ++i)
                {
                    var tokenList = mergedClassDocumentList[i].TokenList;
                    int wordCountOfClass = tokenList.Count(s => s == word);
                    cwp.ConditionalProbabilityList.Add((double)(wordCountOfClass + 1) / (double)(tokenList.Count() + totalDistinctWordCount));
                }
            }

            conditionalWordProbabilityList.Sort((x, y) => x.Word.CompareTo(y.Word));
        }

        public List<double> PriorProbabilitiesList
        {
            get { return priorProbabilitiesList; }
            set { priorProbabilitiesList = value; }
        }

        public List<ConditionalWordProbability> ConditionalWordProbabilityList
        {
            get { return conditionalWordProbabilityList; }
            set { conditionalWordProbabilityList = value; }
        }
    }
}
