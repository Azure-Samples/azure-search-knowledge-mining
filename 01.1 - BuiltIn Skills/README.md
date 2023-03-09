# Adding Built In Skill to the Skillset

Add Sentiment Analysis Skill to the Skillset and verify that sentiment are generated and stored in the index.

Use https://learn.microsoft.com/en-us/azure/search/cognitive-search-skill-sentiment-v3 as reference for Skill inputs and outputs


- Add field `sentiment` to index
```json
 {
      "name": "sentiment",
      "type": "Edm.String",
      "searchable": true,
      "sortable": true,
      "filterable": true,
      "facetable": true
    }
```

- Add `"#Microsoft.Skills.Text.V3.SentimentSkill` to skillset
```json
{
      "@odata.type": "#Microsoft.Skills.Text.V3.SentimentSkill",
      "name": "sentiment",
      "description": "",
      "context": "/document",
      "defaultLanguageCode": "en",
      "modelVersion": "",
      "includeOpinionMining": true,
      "inputs": [
        {
          "name": "text",
          "source": "/document/merged_text"
        }
      ],
      "outputs": [
        {
          "name": "sentiment",
          "targetName": "sentiment"
        },
        {
          "name": "confidenceScores",
          "targetName": "confidenceScores"
        },
        {
          "name": "sentences",
          "targetName": "sentences"
        }
      ]
    }
```

- Update Indexer to add output mappings between skill output and index field

```json
  {
         "sourceFieldName": "/document/sentiment",
         "targetFieldName": "sentiment"
  }
```

**Refer** to Postman collection for  more details  


# Verify Index data

- Search for all docments that have 'GitHub` word in them sorting by sentiment

- Search all document and show sentiment and locations facets

- Search documents that have location in Europe