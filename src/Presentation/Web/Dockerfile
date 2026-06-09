FROM node:22-alpine AS build
WORKDIR /app

RUN npm install -g pnpm

COPY src/Presentation/Web/package.json src/Presentation/Web/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY src/Presentation/Web/ .
RUN pnpm exec ng build --configuration production --base-href /knowledgehub/

FROM nginx:alpine AS runtime
COPY --from=build /app/dist/Web/browser /usr/share/nginx/html/knowledgehub
COPY infra/configs/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
