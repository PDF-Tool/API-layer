<script setup lang="ts">
import { ref } from 'vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { checkLpdServerConnection } from '@/services/pdf-service'
import { toast } from 'vue-sonner'
import { LoaderCircle, CheckCircle } from 'lucide-vue-next'

const serverHost = ref('localhost')
const serverPort = ref(515)
const isChecking = ref(false)
const isConnected = ref(false)

async function testConnection() {
  isChecking.value = true
  isConnected.value = false
  
  try {
    const result = await checkLpdServerConnection(serverHost.value, serverPort.value)
    isConnected.value = result.connected
    
    if (result.connected) {
      toast.success('Successfully connected to LPD server')
    } else {
      toast.error('Failed to connect to LPD server')
    }
  } catch (error) {
    console.error('Error testing connection:', error)
    toast.error('Error testing connection to LPD server')
  } finally {
    isChecking.value = false
  }
}
</script>

<template>
  <Card>
    <CardHeader>
      <CardTitle>LPD Print Server Configuration</CardTitle>
      <CardDescription>
        Configure the connection to your LPD print server
      </CardDescription>
    </CardHeader>
    <CardContent>
      <div class="grid gap-4">
        <div class="grid gap-2">
          <Label for="host">Server Host</Label>
          <Input id="host" v-model="serverHost" placeholder="localhost" />
        </div>
        <div class="grid gap-2">
          <Label for="port">Server Port</Label>
          <Input id="port" type="number" v-model.number="serverPort" placeholder="515" />
        </div>
      </div>
    </CardContent>
    <CardFooter class="flex justify-between">
      <div class="flex items-center gap-2" v-if="isConnected">
        <CheckCircle class="h-5 w-5 text-green-500" />
        <span class="text-sm text-green-600">Connected</span>
      </div>
      <div v-else></div>
      <Button @click="testConnection" :disabled="isChecking">
        <LoaderCircle v-if="isChecking" class="mr-2 h-4 w-4 animate-spin" />
        <span>Test Connection</span>
      </Button>
    </CardFooter>
  </Card>
</template> 