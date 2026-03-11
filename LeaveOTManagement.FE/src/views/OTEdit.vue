<template>

<div class="page-wrapper">

  <div class="container py-5">

    <div class="card ot-card border-0">

      <!-- HEADER -->

      <div class="card-header ot-header">

        <h4 class="text-white fw-bold mb-0">
          Edit OT Request
        </h4>

      </div>

      <!-- BODY -->

      <div class="card-body">

        <form @submit.prevent="update">

          <div class="row g-4">

            <!-- DATE -->

            <div class="col-md-4">
              <div class="input-group-modern">

                <label>Date</label>

                <input
                  type="date"
                  v-model="form.date"
                  required
                />

              </div>
            </div>

            <!-- FROM -->

            <div class="col-md-4">
              <div class="input-group-modern">

                <label>From</label>

                <input
                  type="time"
                  v-model="form.fromTime"
                  required
                />

              </div>
            </div>

            <!-- TO -->

            <div class="col-md-4">
              <div class="input-group-modern">

                <label>To</label>

                <input
                  type="time"
                  v-model="form.toTime"
                  required
                />

              </div>
            </div>

          </div>

          <!-- OT HOURS PREVIEW -->

          <div class="ot-preview mt-4" v-if="hours > 0">

            ⏱ Estimated OT Hours :
            <strong>{{ hours }} hours</strong>

          </div>

          <!-- REASON -->

          <div class="mt-4">

            <div class="input-group-modern">

              <label>Reason</label>

              <textarea
                rows="3"
                v-model="form.reason"
                placeholder="Explain your overtime reason..."
                required
              ></textarea>

            </div>

          </div>

          <!-- BUTTONS -->

          <div class="mt-4 d-flex gap-3">

            <button
              class="btn-modern"
              :disabled="loading"
            >

              <span v-if="loading"
              class="spinner-border spinner-border-sm me-2"></span>

              {{ loading ? "Updating..." : "Update OT" }}

            </button>

            <router-link
              to="/my-ot"
              class="btn-back"
            >

              Back

            </router-link>

          </div>

        </form>

      </div>

    </div>

  </div>

</div>

</template>

<script setup>
import { reactive, onMounted, ref, computed } from "vue"
import { useRoute, useRouter } from "vue-router"
import api from "@/services/api"

const route = useRoute()
const router = useRouter()

const loading = ref(false)

const form = reactive({
  date: "",
  fromTime: "",
  toTime: "",
  reason: ""
})

/* calculate OT hours */

const hours = computed(() => {

  if(!form.fromTime || !form.toTime) return 0

  const start = new Date(`2000-01-01T${form.fromTime}`)
  const end = new Date(`2000-01-01T${form.toTime}`)

  const diff = (end - start) / (1000*60*60)

  return diff > 0 ? diff.toFixed(2) : 0

})

/* LOAD OT */

onMounted(async () => {

  const res = await api.get(`/OT/${route.params.id}`)
  const data = res.data

  const detail = data.details?.[0]

  if (detail) {

    form.date = detail.workDate?.split("T")[0]
    form.fromTime = detail.fromTime?.substring(0,5)
form.toTime = detail.toTime?.substring(0,5)

  }

  // reason nằm ở OTRequest
  form.reason = data.reason

})

/* UPDATE */

const update = async () => {

  loading.value = true

  try {

    const payload = {

      reason: form.reason,

      details: [
        {
          workDate: form.date,
          fromTime: form.fromTime + ":00",
          toTime: form.toTime + ":00"
        }
      ]

    }

    await api.put(`/OT/${route.params.id}`, payload)

    window.$toast("OT updated successfully", "success")

    setTimeout(() => {
      router.push("/my-ot")
    }, 800)

  }

  catch(err){

    console.error(err)

    window.$toast("Update failed", "error")

  }

  finally{

    loading.value = false

  }

}
</script>

<style scoped>

/* PAGE BACKGROUND */

.page-wrapper{

background: linear-gradient(135deg,#eef2ff,#f7f9ff);
min-height:100vh;

animation: fadePage 0.6s ease;

}

@keyframes fadePage{

from{
opacity:0;
transform:translateY(20px);
}

to{
opacity:1;
transform:translateY(0);
}

}

/* CARD */

.ot-card{

border-radius:18px;

background: rgba(255,255,255,0.75);

backdrop-filter: blur(10px);

box-shadow:0 20px 50px rgba(0,0,0,0.1);

overflow:hidden;

}

/* HEADER */

.ot-header{

background: linear-gradient(135deg,#5b7cfa,#6a5cff);

padding:18px;

animation: gradientMove 6s infinite alternate;

}

@keyframes gradientMove{

0%{filter:hue-rotate(0deg)}
100%{filter:hue-rotate(30deg)}

}

/* INPUT GROUP */

.input-group-modern{

display:flex;

flex-direction:column;

gap:6px;

}

.input-group-modern input,
.input-group-modern textarea{

border-radius:10px;

border:1px solid #e0e6ff;

padding:10px;

transition:0.25s;

font-size:14px;

}

.input-group-modern input:focus,
.input-group-modern textarea:focus{

border-color:#5b7cfa;

box-shadow:0 0 0 3px rgba(91,124,250,0.2);

}

/* BUTTON */

.btn-modern{

background:linear-gradient(135deg,#5b7cfa,#6a5cff);

color:white;

border:none;

border-radius:12px;

padding:10px 24px;

font-weight:600;

transition:0.25s;

}

.btn-modern:hover{

transform:translateY(-3px);

box-shadow:0 10px 20px rgba(0,0,0,0.2);

}

/* BACK */

.btn-back{

padding:10px 20px;

border-radius:12px;

border:1px solid #ddd;

text-decoration:none;

color:#333;

transition:0.25s;

}

.btn-back:hover{

background:#f5f6ff;

}

/* OT PREVIEW */

.ot-preview{

background:#eef3ff;

padding:12px;

border-radius:10px;

font-size:14px;

color:#3b5bdb;

animation: fadePreview 0.4s ease;

}

@keyframes fadePreview{

from{
opacity:0;
transform:translateY(-10px);
}

to{
opacity:1;
transform:translateY(0);
}

}

</style>