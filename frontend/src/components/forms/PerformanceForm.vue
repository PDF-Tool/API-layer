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
import { fileSizeFormats, metrics } from '@/lib/utils'
import { pageFormats } from '@/lib/pageFormat'
import { Label } from '@/components/ui/label'
import { computed, ref } from 'vue'
import { usePdfStore } from '@/stores/pdfStore'
import type { PdfFields, PerformancePdfFields } from '@/types/pdf'

const { createAndPrintPdf } = usePdfStore()

const canCreatePdf = computed(() => {
    return formData.value.Duration && formData.value.SizePerPage && formData.value.Host
})

const formData = ref<PerformancePdfFields>({
    Duration: 0,
    SizePerPage: 1,
    ByteUnit: 'MB',
    Host: '',
})
</script>

<template>
    <div class="flex flex-col gap-4">
        <div class="grid w-full items-center gap-2">
            <Label for="size">Size per page</Label>
            <div class="flex gap-2">
                <Input type="number" v-model="formData.SizePerPage" name="sizePerPage"
                    placeholder="Select image size per page..." />
                <DropdownMenu>
                    <DropdownMenuTrigger as-child>
                        <Button class="min-w-[70px] select-none"> {{ formData.ByteUnit }}
                            <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent>
                        <DropdownMenuRadioGroup v-model="formData.ByteUnit">
                            <DropdownMenuRadioItem v-for="format in fileSizeFormats" :key="format" :value="format">
                                {{ format }}
                            </DropdownMenuRadioItem>
                        </DropdownMenuRadioGroup>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
        </div>
        <div class="grid w-full items-center gap-2">
            <Label for="pages">Duration (in minutes)</Label>
            <Input type="number" name="pages" v-model="formData.Duration" placeholder="Select amount of pages..." />
        </div>
        <div class="grid w-full items-center gap-2">
            <Label for="pages">Host/IP</Label>
            <Input type="text" name="Host" v-model="formData.Host" placeholder="Select host address..." />
        </div>
        <Button :disabled="!canCreatePdf" @click="createAndPrintPdf">Generate PDF</Button>
    </div>
</template>