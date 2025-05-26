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
import { Label } from '@/components/ui/label'
import { computed, ref } from 'vue'
import { usePdfStore } from '@/stores/pdfStore'
import type { BatchPdfFields } from '@/types/pdf'

const { createAndPrintBatchPdf } = usePdfStore()

const batchFormData = ref<BatchPdfFields>({
    NumberOfFiles: 1,
    PagesPerFile: 1,
    SizePerPage: 1,
    ByteUnit: 'MB',
    Host: '',
})

const canCreateBatchPdf = computed(() => {
    return batchFormData.value.NumberOfFiles && batchFormData.value.PagesPerFile && batchFormData.value.SizePerPage && batchFormData.value.Host
})
</script>

<template>
    <div class="flex flex-col gap-4">
        <div class="grid w-full items-center gap-2">
            <Label for="numberOfFiles">Amount of documents</Label>
            <Input type="number" v-model="batchFormData.NumberOfFiles" name="numberOfFiles"
                placeholder="Select amount of documents..." />
        </div>
        <div class="grid w-full items-center gap-2">
            <Label for="sizePerPage">Size per page
                <Info
                    text="Select the amount of bytes per page in the PDF document, Select in the dropdown-menu in which size this needs to be." />
            </Label>
            <div class="flex gap-2">
                <Input type="number" v-model="batchFormData.SizePerPage" name="sizePerPage"
                    placeholder="Select image size per page..." />
                <DropdownMenu>
                    <DropdownMenuTrigger as-child>
                        <Button class="min-w-[70px] select-none"> {{ batchFormData.ByteUnit }}
                            <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent>
                        <DropdownMenuRadioGroup v-model="batchFormData.ByteUnit">
                            <DropdownMenuRadioItem v-for="format in fileSizeFormats" :key="format" :value="format">
                                {{ format }}
                            </DropdownMenuRadioItem>
                        </DropdownMenuRadioGroup>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
        </div>
        <div class="grid w-full items-center gap-2">
            <Label for="pagesPerFile">Amount of pages per document
                <Info text="Select the amount of pages per documen." />
            </Label>
            <Input type="number" name="pagesPerFile" v-model="batchFormData.PagesPerFile"
                placeholder="Select amount of pages..." />
        </div>
        <div class="grid w-full items-center gap-2">
            <Label for="pages">Host/IP
                <Info text="Select the printer host address." />
            </Label>
            <Input type="text" name="Host" v-model="batchFormData.Host" placeholder="Select host address..." />
        </div>
        <Button :disabled="!canCreateBatchPdf" @click="createAndPrintBatchPdf(batchFormData)">Generate Batch
            PDFs</Button>
    </div>
</template>