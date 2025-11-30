# Stage 1: Build
FROM node:20-bookworm-slim AS build

WORKDIR /app

# 1. Копіюємо ВЕСЬ проект з папки Front
COPY Front/ ./

# 2. ! КРИТИЧНИЙ МОМЕНТ !
# Примусово видаляємо node_modules та lock-файл, які могли скопіюватися з Windows.
# Це гарантує, що конфлікт версій (0.25.9 vs 0.25.12) зникне.
RUN rm -rf node_modules package-lock.json

# 3. Встановлюємо залежності начисто для Linux
RUN npm install

# 4. Білд
RUN npm run build

# Stage 2: Production
FROM nginx:stable-alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]