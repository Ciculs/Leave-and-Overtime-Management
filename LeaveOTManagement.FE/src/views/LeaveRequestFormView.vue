<template>
  <div class="card shadow-sm mt-4 border-0">
    <div class="card-header bg-primary text-white">
      <h4 class="mb-0">Create Leave Request (US01)</h4>
    </div>
    
    <div class="card-body p-4">
      <form @submit.prevent="submitLeave">
        <div class="row mb-3">
          <div class="col-md-6">
            <label class="form-label fw-bold">From Date:</label>
            <input type="date" class="form-control" v-model="fromDate" :min="minDate" required>
          </div>
          <div class="col-md-6">
            <label class="form-label fw-bold">To Date:</label>
            <input type="date" class="form-control" v-model="toDate" :min="fromDate || minDate" required>
          </div>
        </div>

        <div v-if="displayError" class="alert alert-danger py-2">
          <i class="bi bi-exclamation-triangle-fill"></i> {{ displayError }}
        </div>
        
        <div v-if="totalDays > 0 && !displayError" class="alert alert-info py-2">
          Requested days: <strong>{{ totalDays }} days</strong> (Your balance: {{ availableBalance }} days).
        </div>

        <div class="mb-4">
          <label class="form-label fw-bold">Reason for leave:</label>
          <textarea class="form-control" rows="3" v-model="reason" placeholder="Example: Family vacation..." required></textarea>
        </div>

        <div class="d-flex justify-content-between">
          <button type="button" class="btn btn-outline-secondary" @click="goBack" :disabled="isSubmitting">Back</button>
          
          <button type="submit" class="btn btn-primary px-4" :disabled="!isFormValid || isSubmitting">
            <span v-if="isSubmitting" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
            {{ isSubmitting ? 'Submitting...' : 'Submit Leave Request' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getLeaveBalance, createLeaveRequest } from '../services/leaveService'

const router = useRouter()

const fromDate = ref('')
const toDate = ref('')
const reason = ref('')
const availableBalance = ref(0) 

const isSubmitting = ref(false)
const apiError = ref('')

onMounted(async () => {
  try {
    const response = await getLeaveBalance(3)
    availableBalance.value = response.data.balance
  } catch (error) {
    console.error('Error fetching balance:', error)
  }
})

const minDate = computed(() => {
  const today = new Date()
  const offset = today.getTimezoneOffset() * 60000
  return (new Date(today - offset)).toISOString().split('T')[0]
})

const totalDays = computed(() => {
  if (!fromDate.value || !toDate.value) return 0
  const start = new Date(fromDate.value)
  const end = new Date(toDate.value)
  if (end < start) return 0
  
  const diffTime = Math.abs(end - start)
  return Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1
})

const uiValidationError = computed(() => {
  apiError.value = '' 
  if (fromDate.value) {
    if (fromDate.value < minDate.value) return 'Start date cannot be in the past.'
    if (toDate.value) {
      if (toDate.value < fromDate.value) return 'End date cannot be earlier than start date.'
      if (totalDays.value > availableBalance.value) {
        return `Insufficient leave balance! You requested ${totalDays.value} days but only have ${availableBalance.value} days left.`
      }
    }
  }
  return ''
})

const displayError = computed(() => uiValidationError.value || apiError.value)

const isFormValid = computed(() => {
  return fromDate.value && toDate.value && reason.value.trim() !== '' && uiValidationError.value === ''
})

const submitLeave = async () => {
  if (!isFormValid.value) return
  
  isSubmitting.value = true
  try {
    const payload = {
      userId: 3, 
      fromDate: fromDate.value,
      toDate: toDate.value,
      reason: reason.value
    }
    
    const response = await createLeaveRequest(payload)
    
    alert(response.data.message) 
    router.push('/leave') 
    
  } catch (error) {
    if (error.response && error.response.data && error.response.data.message) {
      apiError.value = error.response.data.message 
    } else {
      apiError.value = 'Server connection error.'
    }
  } finally {
    isSubmitting.value = false
  }
}

const goBack = () => router.push('/leave')
</script>