import logging
import json
import os
import logging
import datetime
from json import JSONEncoder
import azure.functions as func
from azure.ai.formrecognizer import FormRecognizerClient
from azure.ai.formrecognizer import FormTrainingClient
from azure.core.credentials import AzureKeyCredential

class DateTimeEncoder(JSONEncoder):
        #Override the default method
        def default(self, obj):
            if isinstance(obj, (datetime.date, datetime.datetime)):
                return obj.isoformat()


def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')
    try:
        body = json.dumps(req.get_json())
        if body:
            logging.info(body)
            result = compose_response(body)
            return func.HttpResponse(result, mimetype="application/json")
        else:
            return func.HttpResponse(
                "Invalid body",
                status_code=400
            )
    except ValueError:
        return func.HttpResponse(
             "Invalid body",
             status_code=400
        )
    except KeyError:
        return func.HttpResponse(
             "Skill configuration error. Endpoint, key and model_id required.",
             status_code=400
        )


def compose_response(json_data):
    values = json.loads(json_data)['values']
    # Prepare the Output before the loop
    results = {}
    results["values"] = []
    endpoint = os.environ["FORMS_RECOGNIZER_ENDPOINT"]
    key = os.environ["FORMS_RECOGNIZER_KEY"]
    form_recognizer_client = FormRecognizerClient(endpoint, AzureKeyCredential(key))
    for value in values:
        output_record = transform_value(value, form_recognizer_client)
        if output_record != None:
            results["values"].append(output_record)
    
    return json.dumps(results, ensure_ascii=False, cls=DateTimeEncoder)


## Perform an operation on a record
def transform_value(value, form_recognizer_client):
    try:
        recordId = value['recordId']
    except AssertionError  as error:
        return None
    # Validate the inputs
    try:         
        assert ('data' in value), "'data' field is required."
        data = value['data']   
        print(data)
        form_url = data["formUrl"]  + data["formSasToken"]   
        print(form_url)
        poller = form_recognizer_client.begin_recognize_content_from_url(form_url)
        pages = poller.result()  
        tables = []
        if not pages:
            print("No pages found in doc")
        else:
            for page in pages:
                if not page.tables:
                    print("No tables on page")
                else:
                    for table in page.tables:
                        cells = []
                        print("Table found on page {}:".format(table.page_number))
                        for cell in table.cells:
                            cells.append(
                                {
                                    "text": cell.text,
                                    "rowIndex": cell.row_index,
                                    "colIndex": cell.column_index,
                                    "confidence": cell.confidence,
                                    "is_header": cell.is_header
                                }
                            )
                        tables.append(
                            {
                                "page_number": table.page_number,
                                "row_count": table.row_count,
                                "column_count": table.column_count,
                                "cells": cells
                            }
                        )
    except AssertionError  as error:
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Error:" + error.args[0] }   ]       
            })
    except Exception as error:
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Error:" + str(error) }   ]       
            })
    return ({
            "recordId": recordId,   
            "data": {
                "tables": tables
            }
            })