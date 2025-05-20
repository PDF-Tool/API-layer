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
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import SingleForm from '@/components/forms/SingleForm.vue'
import BatchForm from '@/components/forms/BatchForm.vue'

import type { PdfFields } from '@/types/pdf'
import { usePdfStore } from '@/stores/pdfStore'
import RandomForm from '@/components/forms/RandomForm.vue'
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip'


const { formData,  } = usePdfStore()

const canCreatePdf = computed(() => {
  return formData.pages > 0 && formData.sizePerPage > 0
})
</script>

<template>
  <section>
    <div class="flex flex-col gap-4">
      <Tabs default-value="single" class="w-full">
                <TabsList class="w-full">
          <TooltipProvider>
            <Tooltip>
              <TooltipTrigger asChild>
                <TabsTrigger value="single">
                  Single
                </TabsTrigger>
              </TooltipTrigger>
              <TooltipContent>
                <p>Generate a single PDF document at once</p>
              </TooltipContent>
            </Tooltip>
          </TooltipProvider>
          
          <TooltipProvider>
            <Tooltip>
              <TooltipTrigger asChild>
                <TabsTrigger value="batch">
                  Batch
                </TabsTrigger>
              </TooltipTrigger>
              <TooltipContent>
                <p>Generate multiple PDFs documents at once</p>
              </TooltipContent>
            </Tooltip>
          </TooltipProvider>
          <TooltipProvider>
            <Tooltip>
              <TooltipTrigger asChild>
                <TabsTrigger value="random">
                  Random
                </TabsTrigger>
              </TooltipTrigger>
              <TooltipContent>
                <p>Generate PDFs documents with randomized properties</p>
              </TooltipContent>
            </Tooltip>
          </TooltipProvider>
        </TabsList>
        <TabsContent value="single">
          <SingleForm />
        </TabsContent>
        <TabsContent value="batch">
          <BatchForm />
        </TabsContent>
        <TabsContent value="random">
          <RandomForm />
        </TabsContent>
      </Tabs>
    </div>
  </section>
</template>
