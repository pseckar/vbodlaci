import { useState } from "react";

type Page = "home" | "breathwork" | "course";

type Course = {
    type: string;
    title: string;
    date: string;
    place: string;
};

const services = [
    { title: "Breathwork v bodláčí", short: "Dech jako cesta k větší přítomnosti, zklidnění a jemnějšímu kontaktu se sebou." },
    { title: "Koně v bodláčí", short: "Setkání s koňmi jako prostor pro vztah, vnímání hranic a přirozenou práci s pozorností." },
    { title: "Veterina v bodláčí", short: "Citlivá péče propojená se zkušeností, respektem ke zvířeti a důrazem na celek." },
];

const homeCourses: Course[] = [
    { type: "Breathwork", title: "Večerní dechový kruh", date: "14. května 2026", place: "Brno" },
    { type: "Koně", title: "Den s koňmi v krajině", date: "21. května 2026", place: "Vysočina" },
    { type: "Veterina", title: "Seminář citlivé péče", date: "4. června 2026", place: "Olomouc" },
];

const breathworkCourses: Course[] = [
    { type: "Breathwork", title: "Večerní dechový kruh", date: "14. května 2026", place: "Brno" },
    { type: "Breathwork", title: "Sobotní hluboký breathwork", date: "30. května 2026", place: "Praha" },
    { type: "Breathwork", title: "Individuální dechové sezení", date: "dle domluvy", place: "online / osobně" },
];

const reviews = [
    { name: "Klára", text: "Velmi jemný a bezpečný prostor, ve kterém jsem se mohla opravdu zastavit a nadechnout." },
    { name: "Martin", text: "Přirozené, klidné a přitom silné. Odnášel jsem si větší jasnost i větší kontakt se sebou." },
    { name: "Eva", text: "Citlivý a profesionální přístup. Všechno působilo klidně, srozumitelně a důvěryhodně." },
];

function Footer() {
    return (
        <footer className="border-t border-white/60 py-8 text-sm text-[#5f685d] md:py-10">
            <div className="flex flex-col gap-5 md:flex-row md:items-center md:justify-between">
                <div>
                    <div className="font-serif text-2xl text-[#2f362f]">V bodláčí</div>
                    <div className="mt-1">Breathwork v bodláčí · Koně v bodláčí · Veterina v bodláčí</div>
                </div>
                <div className="flex flex-wrap items-center gap-3 md:justify-end">
                    <a href="#" className="rounded-full border border-[#d7cfdf] bg-white/80 px-4 py-2 text-sm text-[#5f5472] shadow-sm">
                        Facebook
                    </a>
                    <a href="#" className="rounded-full border border-[#d7cfdf] bg-white/80 px-4 py-2 text-sm text-[#5f5472] shadow-sm">
                        Instagram
                    </a>
                    <span className="hidden text-[#8a8f84] md:inline">•</span>
                    <div>kontakt@vbodlaci.cz</div>
                </div>
            </div>
        </footer>
    );
}

function CourseCard({ course, onOpen }: { course: Course; onOpen: () => void }) {
    return (
        <div className="rounded-[2rem] border border-white/80 bg-white/75 p-6 shadow-lg shadow-[#8d7fa2]/10 backdrop-blur">
            <div className="flex items-center justify-between gap-3">
        <span className="rounded-full bg-[#f1ebf6] px-3 py-1 text-xs uppercase tracking-[0.24em] text-[#7b6e91]">
          {course.type}
        </span>
                <span className="text-sm text-[#667064]">{course.date}</span>
            </div>
            <h3 className="mt-5 font-serif text-2xl leading-tight">{course.title}</h3>
            <div className="mt-4 text-sm leading-6 text-[#5a625b]">{course.place}</div>
            <button
                type="button"
                onClick={onOpen}
                className="mt-6 rounded-full bg-[#2f4a3a] px-5 py-3 text-sm text-white shadow-lg shadow-[#2f4a3a]/20"
            >
                Detail kurzu
            </button>
        </div>
    );
}

function Faq({ items }: { items: Array<[string, string]> }) {
    return (
        <div className="mt-6 space-y-4">
            {items.map(([q, a]) => (
                <details key={q} className="group rounded-[1.6rem] border border-[#eadfce] bg-white/70 p-5 open:bg-white">
                    <summary className="flex cursor-pointer list-none items-center justify-between gap-4 font-medium text-[#2f362f]">
                        <span>{q}</span>
                        <span className="text-[#8a7c9b]">
              <span className="group-open:hidden">▾</span>
              <span className="hidden group-open:inline">▴</span>
            </span>
                    </summary>
                    <p className="mt-3 text-sm leading-6 text-[#5c5f57]">{a}</p>
                </details>
            ))}
        </div>
    );
}

function HomePage({ onOpenBreathwork, onOpenCourse }: { onOpenBreathwork: () => void; onOpenCourse: () => void }) {
    return (
        <>
            <header className="py-6 text-center md:py-8">
                <h1 className="font-serif text-5xl leading-none md:text-7xl">V bodláčí</h1>
                <p className="mx-auto mt-4 max-w-2xl text-base leading-7 text-[#5f675d] md:text-lg">
                    Tři propojené služby, jedna osoba, jeden přirozený svět.
                </p>
            </header>

            <section className="pb-12 pt-4 md:pb-20 md:pt-6">
                <div className="grid gap-5 lg:grid-cols-[1.1fr_0.95fr_0.95fr]">
                    {services.map((service, i) => (
                        <button
                            type="button"
                            key={service.title}
                            onClick={onOpenBreathwork}
                            className={`group relative min-h-[440px] overflow-hidden rounded-[2.3rem] border border-white/70 text-left shadow-xl backdrop-blur transition hover:-translate-y-1 hover:shadow-2xl ${
                                i === 0
                                    ? "bg-[linear-gradient(180deg,rgba(210,223,208,0.72)_0%,rgba(242,234,224,0.78)_55%,rgba(228,220,239,0.86)_100%)]"
                                    : i === 1
                                        ? "bg-[linear-gradient(180deg,rgba(205,219,202,0.78)_0%,rgba(233,225,215,0.82)_58%,rgba(218,210,231,0.9)_100%)]"
                                        : "bg-[linear-gradient(180deg,rgba(236,239,232,0.82)_0%,rgba(243,234,225,0.82)_60%,rgba(223,216,234,0.9)_100%)]"
                            }`}
                        >
                            <div className="absolute inset-x-0 top-0 h-[58%] bg-black/10" />
                            <div className="relative flex h-full flex-col justify-end p-6 md:p-7">
                                <div className="rounded-[1.7rem] bg-white/72 p-5 shadow-lg backdrop-blur-sm">
                                    <div className="flex items-start justify-between gap-4">
                                        <div>
                                            <div className="font-serif text-[2rem] leading-none md:text-[2.4rem]">
                                                {service.title.split(" v bodláčí")[0]}
                                            </div>
                                            <div className="mt-2 text-base uppercase tracking-[0.18em] text-[#6a6278] md:text-[17px]">
                                                v bodláčí
                                            </div>
                                        </div>
                                        <span className="mt-1 text-lg text-[#7f7293]">↗</span>
                                    </div>
                                    <p className="mt-3 text-sm leading-6 text-[#5c645c] md:text-[15px]">{service.short}</p>
                                    <div className="mt-5 border-t border-[#ebe5f1] pt-4 text-sm font-medium text-[#2f4a3a]">
                                        Zjistit víc o službě
                                    </div>
                                </div>
                            </div>
                        </button>
                    ))}
                </div>
            </section>

            <section className="pb-12 md:pb-20">
                <div className="grid gap-6 lg:grid-cols-[1.05fr_0.95fr]">
                    <div className="rounded-[2.25rem] border border-white/60 bg-[linear-gradient(180deg,#f8f4ec_0%,#ede4f3_100%)] p-8 shadow-xl shadow-[#907ea8]/10 md:p-10">
                        <h2 className="font-serif text-3xl leading-tight md:text-5xl">Jedna osoba. Tři podoby práce.</h2>
                        <p className="mt-5 max-w-2xl text-base leading-8 text-[#555d56]">
                            Jeden citlivý přístup, který se v různých podobách projevuje skrze dech, práci s koňmi a veterinární péči. Každá oblast má vlastní charakter, ale všechny spojuje stejná osobní značka, podobná atmosféra a důraz na vnímavost, bezpečí a skutečný kontakt.
                        </p>
                    </div>
                    <div className="rounded-[2.25rem] border border-white/70 bg-white/60 p-5 shadow-xl shadow-[#8b809d]/10 backdrop-blur">
                        <div className="flex h-full min-h-[340px] flex-col justify-end rounded-[1.8rem] bg-[linear-gradient(180deg,#d7e0d4_0%,#f4eadf_50%,#dacfe8_100%)] p-6">
                            <div className="max-w-sm rounded-[1.4rem] bg-white/70 p-5">
                                <div className="font-serif text-2xl">Přírodní editorial</div>
                                <p className="mt-3 text-sm leading-6 text-[#5c645d]">
                                    Jemné přechody, botanické linky a klidná atmosféra, která drží celý web pohromadě. Vizuál má působit přirozeně, lehce a důvěryhodně, bez zbytečné okázalosti.
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <section className="pb-14 md:pb-24">
                <h2 className="mb-8 font-serif text-3xl md:text-5xl">Kurzy napříč všemi oblastmi</h2>
                <div className="grid gap-5 lg:grid-cols-3">
                    {homeCourses.map((course) => (
                        <CourseCard key={course.title} course={course} onOpen={onOpenCourse} />
                    ))}
                </div>
            </section>

            <section className="pb-14 md:pb-24">
                <h2 className="font-serif text-3xl md:text-5xl">Ohlasy</h2>
                <div className="mt-8 -mx-4 overflow-x-auto px-4 [scrollbar-width:none] sm:-mx-6 sm:px-6 lg:-mx-8 lg:px-8">
                    <div className="flex w-max gap-5 pb-2">
                        {reviews.map((review, index) => (
                            <div
                                key={`${review.name}-${index}`}
                                className="relative flex min-h-[240px] w-[320px] flex-col rounded-[2.2rem] border border-[#eadfce] bg-[linear-gradient(180deg,#fffaf4_0%,#f7efe5_100%)] p-7 shadow-lg shadow-[#b89b7c]/10 md:w-[390px]"
                            >
                                <div className="absolute right-6 top-5 font-serif text-5xl leading-none text-[#d8c4af]">“</div>
                                <div className="flex-1 text-base leading-8 text-[#5c5f57] md:text-[17px]">{review.text}</div>
                                <div className="mt-8 border-t border-[#eadfce] pt-4 text-sm font-medium text-[#2f362f]">{review.name}</div>
                            </div>
                        ))}
                    </div>
                </div>
            </section>

            <section className="pb-16 md:pb-24">
                <div className="grid gap-6 lg:grid-cols-2">
                    <div className="rounded-[2.25rem] border border-white/70 bg-white/70 p-7 shadow-xl shadow-[#8d7fa2]/10 backdrop-blur md:p-8">
                        <h2 className="font-serif text-3xl md:text-4xl">Kontakt</h2>
                        <p className="mt-4 max-w-lg text-sm leading-7 text-[#5e665d]">
                            Pro otázky, individuální domluvu nebo rezervaci napiš zprávu. Kontakt má působit jednoduše, lidsky a bez zbytečných bariér, aby bylo snadné udělat první krok.
                        </p>
                        <div className="mt-6 grid gap-4">
                            <input className="rounded-2xl border border-[#ddd6e6] bg-[#fcfbf8] px-5 py-4 text-sm outline-none" placeholder="Jméno" />
                            <input className="rounded-2xl border border-[#ddd6e6] bg-[#fcfbf8] px-5 py-4 text-sm outline-none" placeholder="E-mail" />
                            <textarea className="min-h-[140px] rounded-2xl border border-[#ddd6e6] bg-[#fcfbf8] px-5 py-4 text-sm outline-none" placeholder="Zpráva" />
                            <button className="w-full rounded-full bg-[#2f4a3a] px-6 py-4 text-sm text-white shadow-lg shadow-[#2f4a3a]/20 md:w-fit">Odeslat zprávu</button>
                        </div>
                    </div>
                    <div className="rounded-[2.25rem] border border-white/70 bg-[linear-gradient(180deg,#f7f2ea_0%,#ece4f3_100%)] p-7 shadow-xl shadow-[#8d7fa2]/10 md:p-8">
                        <h2 className="font-serif text-3xl md:text-4xl">Newsletter</h2>
                        <p className="mt-4 max-w-lg text-sm leading-7 text-[#5d655d]">
                            Přihlášení k odběru novinek o nových termínech, kurzech a dění napříč všemi třemi oblastmi značky.
                        </p>
                        <div className="mt-8 rounded-[1.8rem] bg-white/75 p-5 shadow-sm backdrop-blur-sm">
                            <label className="text-sm text-[#61695f]">Tvůj e-mail</label>
                            <input className="mt-3 w-full rounded-2xl border border-[#ddd6e6] bg-[#fcfbf8] px-5 py-4 text-sm outline-none" placeholder="jmeno@email.cz" />
                            <div className="mt-5">
                                <div className="text-sm text-[#61695f]">Zajímá mě</div>
                                <div className="mt-3 space-y-3">
                                    {services.map((item) => (
                                        <label key={item.title} className="flex items-center gap-3 text-sm text-[#5f675d]">
                                            <input type="checkbox" defaultChecked className="h-4 w-4 rounded border-[#cfc6db]" />
                                            <span>{item.title}</span>
                                        </label>
                                    ))}
                                </div>
                            </div>
                            <button className="mt-6 w-full rounded-full bg-[#2f4a3a] px-6 py-4 text-sm text-white shadow-lg shadow-[#2f4a3a]/20">Přihlásit k odběru</button>
                        </div>
                    </div>
                </div>
            </section>

            <Footer />
        </>
    );
}

function BreathworkPage({ onBackHome, onOpenCourse }: { onBackHome: () => void; onOpenCourse: () => void }) {
    return (
        <>
            <header className="py-6 md:py-8">
                <button type="button" onClick={onBackHome} className="rounded-full border border-[#d7cfdf] bg-white/80 px-4 py-2 text-sm text-[#5f5472] shadow-sm">
                    ← Zpět na hlavní stránku
                </button>
                <h1 className="mt-6 font-serif text-5xl leading-none md:text-7xl">Breathwork</h1>
                <div className="mt-4 text-xl uppercase tracking-[0.18em] text-[#6a6278] md:text-[24px]">v bodláčí</div>
            </header>

            <section className="pb-12 md:pb-20">
                <div className="grid gap-6 lg:grid-cols-[1.08fr_0.92fr]">
                    <div className="rounded-[2.25rem] border border-white/70 bg-[linear-gradient(180deg,#edf2e8_0%,#f5ece2_52%,#e8e0f1_100%)] p-8 shadow-xl shadow-[#907ea8]/10 md:p-10">
                        <h2 className="font-serif text-3xl leading-tight md:text-5xl">Prostor, kde dech není výkon, ale cesta zpět k sobě.</h2>
                        <p className="mt-5 text-base leading-8 text-[#555d56]">
                            Jemný a vědomě vedený prostor pro práci s dechem, regulaci nervového systému a prohlubování kontaktu se sebou. Stránka má působit klidněji než homepage a dát návštěvníkovi jasnější představu o tom, co může čekat.
                        </p>
                    </div>
                    <div className="rounded-[2.25rem] border border-white/70 bg-white/70 p-5 shadow-xl shadow-[#8b809d]/10 backdrop-blur">
                        <div className="flex h-full min-h-[340px] flex-col justify-end rounded-[1.8rem] bg-[linear-gradient(180deg,#dbe7d7_0%,#f3e9df_48%,#dfd6eb_100%)] p-6">
                            <div className="max-w-sm rounded-[1.4rem] bg-white/72 p-5">
                                <div className="font-serif text-2xl">Klidná atmosféra</div>
                                <p className="mt-3 text-sm leading-6 text-[#5c645d]">
                                    Velká fotka, světlo, dech a klid. Obrazový blok může nést hlavní atmosféru služby a dát stránce víc prostoru, jemnosti a zpomalení.
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <section className="pb-12 md:pb-20">
                <div className="grid gap-6 lg:grid-cols-[0.95fr_1.05fr]">
                    <div className="rounded-[2rem] border border-white/70 bg-white/70 p-7 shadow-lg shadow-[#8d7fa2]/10 backdrop-blur md:p-8">
                        <h2 className="font-serif text-3xl md:text-4xl">Pro koho je breathwork</h2>
                        <div className="mt-6 space-y-4 text-sm leading-7 text-[#596259]">
                            <p>Pro lidi, kteří chtějí zpomalit, lépe vnímat tělo a vytvořit si větší kapacitu pro každodenní život.</p>
                            <p>Pro jednotlivce i malé skupiny, které ocení citlivé vedení, bezpečný rámec a prostor pro vlastní tempo.</p>
                        </div>
                    </div>
                    <div className="rounded-[2rem] border border-white/70 bg-[linear-gradient(180deg,#faf6ef_0%,#f2e8f6_100%)] p-7 shadow-lg shadow-[#8d7fa2]/10 md:p-8">
                        <h2 className="font-serif text-3xl md:text-4xl">Co může přinést</h2>
                        <div className="mt-6 grid gap-3 sm:grid-cols-2">
                            {[
                                "zklidnění a větší vnitřní stabilitu",
                                "hlubší kontakt se sebou a svým tělem",
                                "bezpečný prostor pro zastavení a vnímání",
                                "uvolnění napětí a jemnější návrat k sobě",
                            ].map((benefit) => (
                                <div key={benefit} className="rounded-2xl bg-white/78 p-4 text-sm leading-6 text-[#596259]">{benefit}</div>
                            ))}
                        </div>
                    </div>
                </div>
            </section>

            <section className="pb-12 md:pb-20">
                <div className="rounded-[2.25rem] border border-white/70 bg-white/70 p-7 shadow-xl shadow-[#8d7fa2]/10 backdrop-blur md:p-8">
                    <h2 className="font-serif text-3xl md:text-5xl">Jak setkání probíhá</h2>
                    <div className="mt-8 grid gap-4 md:grid-cols-4">
                        {[
                            ["1", "Naladění", "Krátký úvod, zklidnění a vytvoření bezpečného prostoru pro celé setkání."],
                            ["2", "Dech", "Vedená práce s dechem v citlivém tempu, které respektuje skupinu i jednotlivce."],
                            ["3", "Integrace", "Čas na doznění, návrat do těla a jemné usazení prožitku."],
                            ["4", "Závěr", "Prostor pro krátké sdílení, otázky a uzavření společného času."],
                        ].map(([num, title, text]) => (
                            <div key={num} className="rounded-[1.6rem] bg-[#faf7f2] p-5">
                                <div className="text-xs uppercase tracking-[0.22em] text-[#8a7c9b]">Krok {num}</div>
                                <div className="mt-3 font-serif text-2xl">{title}</div>
                                <p className="mt-3 text-sm leading-6 text-[#5d655d]">{text}</p>
                            </div>
                        ))}
                    </div>
                </div>
            </section>

            <section className="pb-14 md:pb-24">
                <h2 className="mb-8 font-serif text-3xl md:text-5xl">Nejbližší breathwork termíny</h2>
                <div className="grid gap-5 lg:grid-cols-3">
                    {breathworkCourses.map((course) => (
                        <CourseCard key={course.title} course={course} onOpen={onOpenCourse} />
                    ))}
                </div>
            </section>

            <section className="pb-12 md:pb-20">
                <div className="rounded-[2.25rem] border border-white/70 bg-[linear-gradient(180deg,#fffaf4_0%,#f7efe5_100%)] p-7 shadow-xl shadow-[#b89b7c]/10 md:p-8">
                    <h2 className="font-serif text-3xl md:text-4xl">Časté otázky</h2>
                    <Faq items={[["Musím mít předchozí zkušenost s breathwork?", "Ne, setkání může být vhodné i pro úplné začátečníky. Důležité je jen přijít s otevřeností a ochotou vnímat vlastní tempo."], ["Je breathwork fyzicky nebo emočně náročný?", "Podoba práce může být jemná a citlivě vedená. Vždy záleží na konkrétním formátu, skupině i aktuálním rozpoložení účastníka."], ["Můžu se předem na něco zeptat nebo ověřit vhodnost?", "Ano, právě k tomu slouží kontakt na stránce. Je v pořádku napsat předem a ujasnit si, jestli je tahle forma setkání pro tebe vhodná."]]} />
                </div>
            </section>

            <section className="pb-16 md:pb-24">
                <div className="grid gap-6 lg:grid-cols-2">
                    <div className="rounded-[2.25rem] border border-white/70 bg-white/70 p-7 shadow-xl shadow-[#8d7fa2]/10 backdrop-blur md:p-8">
                        <h2 className="font-serif text-3xl md:text-4xl">Kontakt k breathwork</h2>
                        <div className="mt-6 grid gap-4">
                            <input className="rounded-2xl border border-[#ddd6e6] bg-[#fcfbf8] px-5 py-4 text-sm outline-none" placeholder="Jméno" />
                            <input className="rounded-2xl border border-[#ddd6e6] bg-[#fcfbf8] px-5 py-4 text-sm outline-none" placeholder="E-mail" />
                            <textarea className="min-h-[140px] rounded-2xl border border-[#ddd6e6] bg-[#fcfbf8] px-5 py-4 text-sm outline-none" placeholder="Dotaz" />
                            <button className="w-full rounded-full bg-[#2f4a3a] px-6 py-4 text-sm text-white shadow-lg shadow-[#2f4a3a]/20 md:w-fit">Poslat dotaz</button>
                        </div>
                    </div>
                    <div className="rounded-[2.25rem] border border-white/70 bg-[linear-gradient(180deg,#f7f2ea_0%,#ece4f3_100%)] p-7 shadow-xl shadow-[#8d7fa2]/10 md:p-8">
                        <h2 className="font-serif text-3xl md:text-4xl">Newsletter</h2>
                        <div className="mt-8 rounded-[1.8rem] bg-white/75 p-5 shadow-sm backdrop-blur-sm">
                            <label className="text-sm text-[#61695f]">Tvůj e-mail</label>
                            <input className="mt-3 w-full rounded-2xl border border-[#ddd6e6] bg-[#fcfbf8] px-5 py-4 text-sm outline-none" placeholder="jmeno@email.cz" />
                            <button className="mt-4 w-full rounded-full bg-[#2f4a3a] px-6 py-4 text-sm text-white shadow-lg shadow-[#2f4a3a]/20">Přihlásit k odběru</button>
                        </div>
                    </div>
                </div>
            </section>

            <Footer />
        </>
    );
}

function CoursePage({ onBack }: { onBack: () => void }) {
    return (
        <>
            <header className="py-6 md:py-8">
                <button type="button" onClick={onBack} className="rounded-full border border-[#d7cfdf] bg-white/80 px-4 py-2 text-sm text-[#5f5472] shadow-sm">
                    ← Zpět na hlavní stránku
                </button>
                <div className="mt-6 text-xl uppercase tracking-[0.18em] text-[#6a6278] md:text-[24px]">breathwork kurz</div>
                <h1 className="mt-3 font-serif text-4xl leading-tight md:text-6xl">Večerní dechový kruh</h1>
            </header>

            <section className="pb-12 md:pb-20">
                <div className="grid gap-6 lg:grid-cols-[1.05fr_0.95fr]">
                    <div className="rounded-[2.25rem] border border-white/70 bg-[linear-gradient(180deg,#edf2e8_0%,#f5ece2_52%,#e8e0f1_100%)] p-8 shadow-xl shadow-[#907ea8]/10 md:p-10">
                        <h2 className="font-serif text-3xl leading-tight md:text-5xl">Jemně vedený večerní breathwork.</h2>
                        <p className="mt-5 text-base leading-8 text-[#555d56]">
                            Konkrétní večerní setkání zaměřené na dech, zklidnění a postupné usazení do těla. Detail kurzu má dát rychlou a srozumitelnou odpověď na to, co návštěvníka čeká a jestli je kurz pro něj vhodný.
                        </p>
                    </div>
                    <div className="rounded-[2.25rem] border border-white/70 bg-white/70 p-5 shadow-xl shadow-[#8b809d]/10 backdrop-blur">
                        <div className="flex h-full min-h-[340px] flex-col justify-end rounded-[1.8rem] bg-[linear-gradient(180deg,#dbe7d7_0%,#f3e9df_48%,#dfd6eb_100%)] p-6">
                            <div className="max-w-sm rounded-[1.4rem] bg-white/72 p-5">
                                <div className="font-serif text-2xl">Klidný večerní formát</div>
                                <p className="mt-3 text-sm leading-6 text-[#5c645d]">
                                    Hero blok s fotkou nebo atmosférou kurzu. Může ukazovat prostor, světlo, detail místa nebo náladu večera, aby detail působil víc konkrétně a živě.
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <section className="pb-12 md:pb-20">
                <div className="grid gap-6 lg:grid-cols-[0.9fr_1.1fr]">
                    <div className="rounded-[2rem] border border-white/70 bg-white/75 p-7 shadow-lg shadow-[#8d7fa2]/10 backdrop-blur md:p-8">
                        <h2 className="font-serif text-3xl md:text-4xl">Základní informace</h2>
                        <div className="mt-6 space-y-4 text-sm leading-7 text-[#596259]">
                            {[["Termín", "14. května 2026"], ["Čas", "18:00–21:00"], ["Místo", "Brno"], ["Cena", "1 900 Kč"], ["Kapacita", "12 míst"]].map(([label, value]) => (
                                <div key={label} className="flex items-start justify-between gap-6 border-b border-[#ece5f3] pb-4 last:border-none last:pb-0">
                                    <span className="text-[#7b6f90]">{label}</span>
                                    <span className="text-right text-[#2f362f]">{value}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                    <div className="rounded-[2rem] border border-white/70 bg-[linear-gradient(180deg,#faf6ef_0%,#f2e8f6_100%)] p-7 shadow-lg shadow-[#8d7fa2]/10 md:p-8">
                        <h2 className="font-serif text-3xl md:text-4xl">Co tě čeká</h2>
                        <div className="mt-6 grid gap-3 sm:grid-cols-2">
                            {["úvodní naladění", "vedená dechová praxe", "čas na doznění", "krátké závěrečné sdílení"].map((item) => (
                                <div key={item} className="rounded-2xl bg-white/78 p-4 text-sm leading-6 text-[#596259]">{item}</div>
                            ))}
                        </div>
                    </div>
                </div>
            </section>

            <section className="pb-16 md:pb-24">
                <div className="rounded-[2.25rem] border border-white/70 bg-white/70 p-7 shadow-xl shadow-[#8d7fa2]/10 backdrop-blur md:p-8">
                    <h2 className="font-serif text-3xl md:text-4xl">Přihlášení na kurz</h2>
                    <p className="mt-4 max-w-2xl text-sm leading-7 text-[#5e665d]">
                        Jednoduchý formulář pro závazné přihlášení na konkrétní termín. Textově může působit klidně a srozumitelně, aby návštěvník přesně věděl, co odesílá.
                    </p>
                    <div className="mt-6 grid max-w-3xl gap-4">
                        <input className="rounded-2xl border border-[#ddd6e6] bg-[#fcfbf8] px-5 py-4 text-sm outline-none" placeholder="Jméno" />
                        <input className="rounded-2xl border border-[#ddd6e6] bg-[#fcfbf8] px-5 py-4 text-sm outline-none" placeholder="E-mail" />
                        <textarea className="min-h-[140px] rounded-2xl border border-[#ddd6e6] bg-[#fcfbf8] px-5 py-4 text-sm outline-none" placeholder="Poznámka" />
                        <label className="flex items-center gap-3 text-sm text-[#5f675d]">
                            <input type="checkbox" className="h-4 w-4 rounded border-[#cfc6db]" />
                            <span>Souhlasím s podmínkami kurzu</span>
                        </label>
                        <button className="w-full rounded-full bg-[#2f4a3a] px-6 py-4 text-sm text-white shadow-lg shadow-[#2f4a3a]/20 md:w-fit">Závazně přihlásit</button>
                    </div>
                </div>
            </section>

            <section className="pb-16 md:pb-24">
                <h2 className="mb-3 font-serif text-3xl md:text-4xl">Další termíny breathwork</h2>
                <p className="mb-8 max-w-2xl text-sm leading-7 text-[#5d655d]">
                    Pod detailem konkrétního kurzu může být užitečné nabídnout i další vypsané termíny, aby uživatel nemusel zpět o úroveň výš a mohl si rovnou vybrat jinou variantu.
                </p>
                <div className="grid gap-5 lg:grid-cols-3">
                    {breathworkCourses.map((course) => (
                        <CourseCard key={`course-detail-${course.title}`} course={course} onOpen={() => {}} />
                    ))}
                </div>
            </section>

            <Footer />
        </>
    );
}

export default function VBodlaciMockup() {
    const [page, setPage] = useState<Page>("home");

    return (
        <div className="min-h-screen bg-[#f6f1e8] font-sans text-[#2f362f]">
            <div className="pointer-events-none absolute inset-0 overflow-hidden">
                <div className="absolute -left-20 -top-20 h-72 w-72 rounded-full bg-[#d9d1e5]/40 blur-3xl" />
                <div className="absolute right-0 top-[24rem] h-96 w-96 rounded-full bg-[#cad7c4]/40 blur-3xl" />
                <div className="absolute bottom-20 left-1/3 h-80 w-80 rounded-full bg-[#dfcfbc]/40 blur-3xl" />
            </div>

            <div className="relative mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
                {page === "home" ? (
                    <HomePage onOpenBreathwork={() => setPage("breathwork")} onOpenCourse={() => setPage("course")} />
                ) : page === "breathwork" ? (
                    <BreathworkPage onBackHome={() => setPage("home")} onOpenCourse={() => setPage("course")} />
                ) : (
                    <CoursePage onBack={() => setPage("home")} />
                )}
            </div>
        </div>
    );
}
