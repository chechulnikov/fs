#!/bin/bash
docker rmi -f fs-tests
docker build -t fs-tests -f ./build/tests.dockerfile .
docker run --rm --name fs-tests fs-tests