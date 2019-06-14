import spacy
import nltk
import pandas as pd
import io
import json

import flask
from flask import Flask, request, jsonify
app = Flask(__name__)

@app.route('/process', methods=['POST'])
def process():
	content = request.json

	nlp = spacy.load('en_core_web_sm')
	from nltk.corpus import stopwords
	stopwords = stopwords.words('english')

	# add some custom stopwords
	stopwords.append('part')
	stopwords.append('group')

	# return just the key phrases (not counts)
	data = {}
	data['values'] = []

	for record in content["values"]:

		recordId = record["recordId"]
		metadataStoragePath = record["data"]["metadata_storage_path"]
		content = record["data"]["text"]
		doc = nlp(content)
		
		# Get noun phrases (NN*) and include adjectives (JJ*)
		cols = ['phrase', 'value']
		lst = []
		for sent in doc.sents:
			nounCount = 0
			phrase = ''
			for token in sent:
				if ((token.tag_[0] == 'N') and (token.tag_[1] == 'N')):
					if (token.text.lower() not in stopwords):
						nounCount += 1
						phrase += ' ' + token.text
				elif ((token.tag_[0] == 'J') and (token.tag_[1] == 'J')):
					if (token.text.lower() not in stopwords):
						phrase += ' ' + token.text
				else:
					if (nounCount > 0):
						lst.append([phrase.lower().strip(), 1])
					phrase = ''
					nounCount = 0
		if (nounCount > 0):
			lst.append([phrase.lower().strip(), 1])

		dfPhraseListGrouped = pd.DataFrame(lst, columns=cols).groupby('phrase').sum()

		for index, row in dfPhraseListGrouped.iterrows():
			print (row.name, row.value)

		document = {}
		document['recordId'] = recordId

		keyphrases = []
		for index, row in dfPhraseListGrouped.iterrows():
			keyphrases.append( row.name)

		document['data'] = {"keyphrases": keyphrases}

		data['values'].append(document)

	return jsonify(data)

if __name__ == '__main__':
	app.run(debug=True, host='0.0.0.0', port=80)
