.PHONY: build-dockers push-dockers deploy-all
.ONESHELL:

build-dockers: 
	@echo "Building docker images"
	@cd src; \
		cd servicea; \
		docker build . -t thorstenhans/otel-sample-service-a:0.0.1
	@cd src; \
		cd serviceb; \
		docker build . -t thorstenhans/otel-sample-service-b:0.0.1
	@cd src; \
		cd loadgenerator; \
		docker build . -t thorstenhans/otel-sample-load-generator:0.0.2


push-dockers:
	@echo "Pushing docker images"
	@docker push thorstenhans/otel-sample-service-a:0.0.1
	@docker push thorstenhans/otel-sample-service-b:0.0.1
	@docker push thorstenhans/otel-sample-load-generator:0.0.2

deploy-all:
	@echo "Adding necessary helm repositories"
	@helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
	@helm repo add jaegertracing https://jaegertracing.github.io/helm-charts

	@echo "Updating helm repositories"
	@helm repo update

	@echo "Installing Jaeger"
	@helm upgrade --install jaeger -n jaeger --create-namespace jaegertracing/jaeger

	@echo "Installing Prometheus"
	@helm upgrade --install prometheus -n prometheus --create-namespace prometheus-community/prometheus

	@echo "Reading Jaegeer service URL"
	
	@echo "Installing Service B"
	@helm install service-b charts/service-b -n services --create-namespace \
		--set jaeger.host=jaeger-agent.jaeger.svc.cluster.local \
		--set jaeger.port=6831
	
	@echo "Installing Service A"
	@helm install service-a charts/service-a -n services --create-namespace \
		--set jaeger.host=jaeger-agent.jaeger.svc.cluster.local \
		--set jaeger.port=6831 \
		--set backend.host=service-b.services.svc.cluster.local \
		--set backend.port=8080

	@echo "Running LoadGernator"
	@kubectl run load-generator -it --rm --namespace services --image=thorstenhans/otel-sample-load-generator:0.0.2 --restart=Never
