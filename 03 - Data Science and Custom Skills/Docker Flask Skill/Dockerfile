FROM python:3.5-slim
RUN apt-get update && apt-get install -y python3-pip
COPY . /storage-emulator
WORKDIR /storage-emulator

# RUN pip3 install pyopenssl
RUN pip3 install -r requirements.txt

RUN python -m nltk.downloader stopwords
RUN python -m nltk.downloader universal_tagset
RUN python -m spacy download en 

EXPOSE 80
ENTRYPOINT ["python"]
CMD ["ws.py"]