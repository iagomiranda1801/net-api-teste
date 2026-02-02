# Railway Deploy - MinhaAPI

## 🚀 Deploy Automático

Sua API será deployada automaticamente quando você fizer push para o GitHub.

## 📝 Passo a Passo

### 1. Fazer commit e push das alterações
```bash
git add .
git commit -m "chore: preparar para deploy no Railway"
git push origin main
```

### 2. Criar conta no Railway
- Acesse: https://railway.app
- Faça login com GitHub

### 3. Criar novo projeto
- Clique em "New Project"
- Selecione "Deploy from GitHub repo"
- Escolha o repositório `MinhaApi`

### 4. Adicionar PostgreSQL
- No mesmo projeto, clique em "+ New"
- Selecione "Database" → "PostgreSQL"
- Railway cria automaticamente

### 5. Conectar banco à API
- Clique no serviço da API
- Vá em "Variables"
- Adicione:
  ```
  ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
  ```
  (Railway pega automaticamente do PostgreSQL)

### 6. Configurar variáveis de ambiente
No painel Variables, adicione:
```
ASPNETCORE_ENVIRONMENT=Production
Jwt__Key=SUA_CHAVE_SECRETA_SUPER_FORTE_MINIMO_32_CARACTERES
Jwt__Issuer=MinhaApi
Jwt__Audience=MinhaApiClients
Jwt__ExpiresInHours=2
```

### 7. Deploy!
- Railway faz deploy automático
- Aguarde ~2-3 minutos
- Sua API estará em: `https://seu-projeto.up.railway.app`

## 🔧 Comandos úteis

Depois do primeiro deploy, todo `git push` faz deploy automático!

## 📊 Monitoramento
- Logs: clique no serviço → "Deployments" → "View Logs"
- Métricas: aba "Metrics"

## 💰 Custo
- $5 de crédito grátis por mês
- Suficiente para projetos pequenos/médios
