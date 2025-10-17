#!/bin/bash
# Script to generate OpenAPI specification from the BugMgr API

set -e

# Navigate to project root
cd "$(dirname "$0")/../.."

echo "🔨 Building API project..."
dotnet build API/API.csproj

echo "📝 Generating OpenAPI JSON..."
cd API
swagger tofile --output ../openapi.json bin/Debug/net9.0/API.dll v1
cd ..

echo "✅ Validating generated JSON..."
if cat openapi.json | python3 -m json.tool > /dev/null 2>&1; then
    echo "✅ OpenAPI JSON is valid!"
    
    # Show summary
    echo ""
    echo "📊 Summary:"
    echo "  - File size: $(du -h openapi.json | cut -f1)"
    echo "  - Lines: $(wc -l < openapi.json)"
    
    # Count paths and schemas
    echo "  - Total paths: $(cat openapi.json | python3 -c "import json,sys; data=json.load(sys.stdin); print(len(data['paths']))")"
    echo "  - Total schemas: $(cat openapi.json | python3 -c "import json,sys; data=json.load(sys.stdin); print(len(data.get('components', {}).get('schemas', {})))")"
    echo "  - Total endpoints: $(cat openapi.json | python3 -c "import json,sys; data=json.load(sys.stdin); print(sum(len(data['paths'][path]) for path in data['paths']))")"
    
    echo ""
    echo "✅ OpenAPI specification generated successfully at: openapi.json"
else
    echo "❌ Generated JSON is invalid!"
    exit 1
fi
