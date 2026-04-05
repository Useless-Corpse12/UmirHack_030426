FROM node:25-alpine3.22 as build

WORKDIR /app

COPY package.json .
COPY package-lock.json .

RUN npm i

COPY . .

RUN npm run build

FROM nginx:1.29.7-alpine3.23

RUN apk upgrade --no-cache && \
    apk add --no-cache libgcc libstdc++

COPY ./nginx/nginx.conf /etc/nginx/nginx.conf

RUN rm -rf /usr/share/nginx/html/*

COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

ENTRYPOINT ["nginx", "-g", "daemon off;"]