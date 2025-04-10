<script setup lang="ts">
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { fileSizeFormats, metrics, type FileSizeFormat } from '@/lib/utils'
import { pageFormats, type PageFormat } from '@/lib/pageFormat'
import { Label } from '@/components/ui/label'
import { ref, computed } from 'vue'
import type { FdfFields } from '@/types/pdf'
import { usePdfStore } from '@/stores/pdfStore'

const { formData, createPdf } = usePdfStore()

const canCreatePdf = computed(() => {
  return formData.pages > 0 && formData.size > 0
})
</script>

<template>
  <section>
    <div class="flex flex-col gap-4">
      <div class="grid w-full items-center gap-2">
        <Label for="size">Size per page</Label>
        <div class="flex gap-2">
          <Input type="number" v-model="formData.size" name="size" placeholder="Select image size per page..." />
          <DropdownMenu>
            <DropdownMenuTrigger as-child>
              <Button class="min-w-[70px] select-none"> {{ formData.byteUnit }}
                <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent>
              <DropdownMenuRadioGroup v-model="formData.byteUnit">
                <DropdownMenuRadioItem v-for="format in fileSizeFormats" :key="format" :value="format">
                  {{ format }}
                </DropdownMenuRadioItem>
              </DropdownMenuRadioGroup>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>
      <div class="grid w-full items-center gap-2">
        <Label for="size">Amount of pages</Label>
        <Input type="number" name="size" v-model="formData.pages" placeholder="Select amount of pages..." />
      </div>
      <div class="p-4 bg-primary rounded-lg border-input border shadow-sm flex flex-col gap-4">
        <div class="flex flex-col w-full items-center gap-2">
          <label class="w-full">Preset</label>
          <DropdownMenu class="w-full">
            <DropdownMenuTrigger as-child>
              <Button :disabled="!!formData.width || !!formData.height"
                class="select-none border-1 text-black  bg-background! justify-start hover:bg-transparent  w-full">
                {{ formData.format }}
                <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent class="w-full">
              <DropdownMenuRadioGroup v-model="formData.format">
                <DropdownMenuRadioItem v-for="format in pageFormats" :key="format" :value="format">
                  {{ format }}
                </DropdownMenuRadioItem>
              </DropdownMenuRadioGroup>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
        <div class="grid w-full items-center gap-2">
          <Label for="size">Dimensions</Label>
          <div class="flex gap-2">
            <div class="relative">
              <Input id="size" type="number" v-model="formData.width" class="pl-7 bg-background!" />
              <span class="absolute start-0 inset-y-0 flex items-center justify-center px-2">
                <span class="text-foreground/50">W</span>
              </span>
            </div>
            <div class="relative">
              <Input id="size" type="number" v-model="formData.height" class="pl-7 bg-background!" />
              <span class="absolute start-0 inset-y-0 flex items-center justify-center px-2">
                <span class="text-foreground/50">H</span>
              </span>
            </div>
            <DropdownMenu>
              <DropdownMenuTrigger as-child>
                <Button class="min-w-[70px] select-none"> {{ formData.metrixUnit }}
                  <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent>
                <DropdownMenuRadioGroup v-model="formData.metrixUnit">
                  <DropdownMenuRadioItem v-for="metric in metrics" :key="metric" :value="metric">
                    {{ metric }}
                  </DropdownMenuRadioItem>
                </DropdownMenuRadioGroup>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>
      </div>
      <div class="flex flex-col gap-4">
        <div class="flex flex-col gap-2">
          <Label for="checkHost">Check host connection</Label>
          <div class="flex gap-2">
            <Input id="checkHost" name="checkHost" type="text" placeholder="Enter host address..." />
            <Button>
              <span>Check</span>
            </Button>
          </div>
        </div>
      </div>
      <Button :disabled="!canCreatePdf" @click="createPdf">Generate PDF</Button>
    </div>
  </section>
</template>