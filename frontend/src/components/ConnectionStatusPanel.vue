<script setup lang="ts">
import { usePdfStore } from '@/stores/pdfStore'
import { Button } from '@/components/ui/button'
import { AlertCircle, CheckCircle2, RefreshCw } from 'lucide-vue-next'

const pdfStore = usePdfStore()
const { 
  pdfServiceStatus, 
  lpdServerStatus, 
  checkConnections, 
  isCheckingConnections 
} = pdfStore
</script>

<template>
  <div class="bg-muted/50 rounded-lg p-4 space-y-3">
    <div class="flex justify-between items-center">
      <h3 class="text-lg font-medium">Connection Status</h3>
      <Button variant="outline" size="sm" :disabled="isCheckingConnections" @click="checkConnections">
        <RefreshCw class="h-4 w-4 mr-2" :class="{ 'animate-spin': isCheckingConnections }" />
        Refresh
      </Button>
    </div>
    
    <div class="grid gap-2">
      <div class="flex items-center justify-between bg-background p-3 rounded-md">
        <div class="flex items-center gap-2">
          <CheckCircle2 v-if="pdfServiceStatus.connected" class="h-5 w-5 text-green-500" />
          <AlertCircle v-else class="h-5 w-5 text-red-500" />
          <span>PDF Generator Service</span>
        </div>
        <span class="text-sm text-muted-foreground">{{ pdfServiceStatus.message }}</span>
      </div>
      
      <div class="flex items-center justify-between bg-background p-3 rounded-md">
        <div class="flex items-center gap-2">
          <CheckCircle2 v-if="lpdServerStatus.connected" class="h-5 w-5 text-green-500" />
          <AlertCircle v-else class="h-5 w-5 text-red-500" />
          <span>LPD Print Server</span>
        </div>
        <span class="text-sm text-muted-foreground">{{ lpdServerStatus.message }}</span>
      </div>
    </div>
  </div>
</template> 