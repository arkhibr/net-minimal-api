#!/bin/bash
# Arquivo: setup.sh
# Propósito: Script auxiliar para configurar e executar o projeto
# Uso: ./setup.sh

set -e

echo "=========================================="
echo "Produtos API - Setup Script"
echo "=========================================="
echo ""

# Cores para output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Step 1: Verificar .NET SDK
echo -e "${BLUE}[1/4]${NC} Verificando .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo -e "${YELLOW}⚠ .NET SDK não encontrado!${NC}"
    echo "Instale em: https://dotnet.microsoft.com/download/dotnet/9.0"
    exit 1
fi
DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✓ .NET SDK ${DOTNET_VERSION} encontrado${NC}"

# Step 2: Restaurar dependências
echo ""
echo -e "${BLUE}[2/4]${NC} Restaurando dependências..."
dotnet restore
echo -e "${GREEN}✓ Dependências restauradas${NC}"

# Step 3: Compilar
echo ""
echo -e "${BLUE}[3/4]${NC} Compilando projeto..."
dotnet build --configuration Release
echo -e "${GREEN}✓ Projeto compilado${NC}"

# Step 4: Informações
echo ""
echo -e "${BLUE}[4/4]${NC} Pronto para executar!"
echo ""
echo -e "${GREEN}=========================================="
echo "Próximo passo: dotnet run"
echo "===========================================${NC}"
echo ""
echo "Após inicializar, acesse:"
echo "  📡 API:     http://localhost:5000/api/v1/produtos"
echo "  📚 Swagger: http://localhost:5000"
echo ""
