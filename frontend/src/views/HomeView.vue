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
import PerformanceForm from '@/components/forms/PerformanceForm.vue'

</script>

<template>
  <section>
    <div class="flex flex-col gap-4">
      <Tabs default-value="single" class="w-full">
        <TabsList class="w-full">
          <TabsTrigger value="single">
            Single
          </TabsTrigger>
          <TabsTrigger value="batch">
            Batch
          </TabsTrigger>
          <TabsTrigger value="random">
            Random
          </TabsTrigger>
          <TabsTrigger value="performance">
            Performance
          </TabsTrigger>
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
        <TabsContent value="performance">
          <PerformanceForm />
        </TabsContent>
      </Tabs>
    </div>
  </section>
</template>
