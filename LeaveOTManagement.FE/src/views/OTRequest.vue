<template>
  <div class="card">
    <h2>Register OT</h2>

    <form @submit.prevent="submit">

      <div class="row">

        <div class="field">
          <label>Date</label>
          <input type="date" v-model="form.date" required />
        </div>

        <div class="field">
          <label>From</label>
          <input type="time" v-model="form.fromTime" required />
        </div>

        <div class="field">
          <label>To</label>
          <input type="time" v-model="form.toTime" required />
        </div>

      </div>

      <div class="field">
        <label>Reason</label>
        <textarea v-model="form.reason" required />
      </div>

      <div class="actions">

        <button type="button" class="primary-outline" @click="$emit('cancel')">
          <i class="fas fa-chevron-left me-2"></i>
          Return to List
        </button>

        <button type="submit" class="primary" :disabled="loading">
          <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>

          {{ loading ? "Submitting..." : "Submit" }}
        </button>

      </div>

    </form>
  </div>
</template>

<script setup>
import { reactive, ref } from "vue"
import api from "@/services/api"

const emit = defineEmits(["success", "cancel"])

const loading = ref(false)

const form = reactive({
  date: "",
  fromTime: "",
  toTime: "",
  reason: ""
})

const submit = async () => {

  try {

    if (form.toTime <= form.fromTime) {
      window.$toast("Invalid time range", "warning")
      return
    }

    loading.value = true

    const payload = {

      reason: form.reason,

      details: [
        {
          workDate: form.date + "T00:00:00",
          fromTime: form.fromTime + ":00",
          toTime: form.toTime + ":00"
        }
      ]

    }

    await api.post("/OT", payload)

    window.$toast("OT request created successfully", "success")

    form.date = ""
    form.fromTime = ""
    form.toTime = ""
    form.reason = ""

    emit("success")

  }

  catch (err) {

    console.error(err)
    window.$toast("Submit failed", "error")

  }

  finally {

    loading.value = false

  }

}
</script>

<style scoped>
.card {
  background: white;
  padding: 30px;
  border-radius: 18px;
  box-shadow: 0 15px 40px rgba(0, 0, 0, .08);
}

.row {
  display: flex;
  gap: 15px;
}

.field {
  flex: 1;
  display: flex;
  flex-direction: column;
  margin-bottom: 18px;
}

input,
textarea {
  padding: 10px;
  border-radius: 10px;
  border: 1px solid #ddd;
}

.actions {
  display: flex;
  justify-content: space-between;
  margin-top: 20px;
}

.primary {
  background: linear-gradient(135deg, #5b2cff, #6a5cff);
  color: white;
  border: none;
  padding: 12px 22px;
  border-radius: 12px;
  cursor: pointer;
  font-weight: 600;
}

.primary-outline {
  background: white;
  border: 2px solid #5b2cff;
  color: #5b2cff;
  padding: 12px 22px;
  border-radius: 12px;
  cursor: pointer;
  font-weight: 600;
}

.primary-outline:hover {
  background: #5b2cff;
  color: white;
}
</style>