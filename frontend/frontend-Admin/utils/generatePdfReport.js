import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

const PRIMARY = [37, 99, 235];
const HEADER_TEXT = [255, 255, 255];
const SECTION_COLOR = [30, 64, 175];

function addSectionTitle(doc, title, y) {
  doc.setFontSize(12);
  doc.setFont('helvetica', 'bold');
  doc.setTextColor(...SECTION_COLOR);
  doc.text(title, 14, y);
  doc.setTextColor(0, 0, 0);
  doc.setFont('helvetica', 'normal');
  return y + 6;
}

function safeY(doc, y, needed = 30) {
  if (y + needed > doc.internal.pageSize.getHeight() - 20) {
    doc.addPage();
    return 20;
  }
  return y;
}

export function generateReportPdf(reportData, adminEmail) {
  const doc = new jsPDF({ unit: 'mm', format: 'a4' });
  const pageW = doc.internal.pageSize.getWidth();
  const now = new Date();

  const dateStr = now.toLocaleDateString('es-CR', {
    year: 'numeric', month: 'long', day: 'numeric',
  });
  const timeStr = now.toLocaleTimeString('es-CR', {
    hour: '2-digit', minute: '2-digit',
  });

  // ── Portada ──────────────────────────────────────────────────────────────
  doc.setFillColor(...PRIMARY);
  doc.rect(0, 0, pageW, 50, 'F');

  doc.setFontSize(16);
  doc.setFont('helvetica', 'bold');
  doc.setTextColor(255, 255, 255);
  doc.text('REPORTE GENERAL DE LA PLATAFORMA DE EMPLEO', pageW / 2, 22, { align: 'center' });

  doc.setFontSize(11);
  doc.setFont('helvetica', 'normal');
  doc.text('Informe Administrativo — Sinergia', pageW / 2, 32, { align: 'center' });

  doc.setTextColor(0, 0, 0);
  doc.setFontSize(9);
  const metaY = 60;
  doc.text(`Fecha de generación: ${dateStr}`, 14, metaY);
  doc.text(`Hora de generación: ${timeStr}`, 14, metaY + 6);
  doc.text('Plataforma: Sinergia — Empleabilidad para Jóvenes en Costa Rica', 14, metaY + 12);
  if (adminEmail) {
    doc.text(`Administrador: ${adminEmail}`, 14, metaY + 18);
  }

  doc.setDrawColor(200, 200, 200);
  doc.line(14, metaY + 24, pageW - 14, metaY + 24);

  let y = metaY + 34;

  // ── Sección 1: Resumen General ───────────────────────────────────────────
  y = safeY(doc, y, 50);
  y = addSectionTitle(doc, 'SECCIÓN 1 — RESUMEN GENERAL', y);
  autoTable(doc, {
    startY: y,
    head: [['Métrica', 'Cantidad']],
    body: [
      ['Usuarios Totales', reportData.totalUsers],
      ['Candidatos', reportData.totalCandidates],
      ['Empleadores', reportData.totalEmployers],
      ['Administradores', reportData.totalAdministrators],
    ],
    theme: 'striped',
    headStyles: { fillColor: PRIMARY, textColor: HEADER_TEXT, fontStyle: 'bold' },
    columnStyles: { 1: { halign: 'center' } },
    margin: { left: 14, right: 14 },
  });
  y = doc.lastAutoTable.finalY + 12;

  // ── Sección 2: Estadísticas de Vacantes ──────────────────────────────────
  y = safeY(doc, y, 50);
  y = addSectionTitle(doc, 'SECCIÓN 2 — ESTADÍSTICAS DE VACANTES', y);
  const vacanteBody = [
    ['Vacantes Totales', reportData.totalVacantes],
    ['Vacantes Activas', reportData.activeVacantes],
  ];
  if (reportData.closedVacantes > 0) {
    vacanteBody.push(['Vacantes Cerradas', reportData.closedVacantes]);
  }
  autoTable(doc, {
    startY: y,
    head: [['Métrica', 'Cantidad']],
    body: vacanteBody,
    theme: 'striped',
    headStyles: { fillColor: PRIMARY, textColor: HEADER_TEXT, fontStyle: 'bold' },
    columnStyles: { 1: { halign: 'center' } },
    margin: { left: 14, right: 14 },
  });
  y = doc.lastAutoTable.finalY + 12;

  // ── Sección 3: Estadísticas de Postulaciones ─────────────────────────────
  y = safeY(doc, y, 50);
  y = addSectionTitle(doc, 'SECCIÓN 3 — ESTADÍSTICAS DE POSTULACIONES', y);
  autoTable(doc, {
    startY: y,
    head: [['Métrica', 'Cantidad']],
    body: [
      ['Total de Postulaciones', reportData.totalPostulaciones],
      ['Candidatos que han postulado', reportData.candidatesWithPostulaciones],
      ['Vacantes que han recibido postulaciones', reportData.vacantesWithPostulaciones],
    ],
    theme: 'striped',
    headStyles: { fillColor: PRIMARY, textColor: HEADER_TEXT, fontStyle: 'bold' },
    columnStyles: { 1: { halign: 'center' } },
    margin: { left: 14, right: 14 },
  });
  y = doc.lastAutoTable.finalY + 12;

  // ── Sección 4: Estado de las Postulaciones ───────────────────────────────
  y = safeY(doc, y, 50);
  y = addSectionTitle(doc, 'SECCIÓN 4 — ESTADO DE LAS POSTULACIONES', y);
  if (reportData.postulacionesByStatus.length > 0) {
    autoTable(doc, {
      startY: y,
      head: [['Estado', 'Cantidad']],
      body: reportData.postulacionesByStatus.map((s) => [s.status, s.count]),
      theme: 'striped',
      headStyles: { fillColor: PRIMARY, textColor: HEADER_TEXT, fontStyle: 'bold' },
      columnStyles: { 1: { halign: 'center' } },
      margin: { left: 14, right: 14 },
    });
    y = doc.lastAutoTable.finalY + 12;
  } else {
    doc.setFontSize(9);
    doc.setTextColor(120, 120, 120);
    doc.text('No hay postulaciones registradas.', 14, y);
    doc.setTextColor(0, 0, 0);
    y += 10;
  }

  // ── Sección 5: Candidatos por Provincia ──────────────────────────────────
  y = safeY(doc, y, 50);
  y = addSectionTitle(doc, 'SECCIÓN 5 — DISTRIBUCIÓN DE CANDIDATOS POR PROVINCIA', y);
  if (reportData.candidatesByProvince.length > 0) {
    autoTable(doc, {
      startY: y,
      head: [['Provincia', 'Candidatos']],
      body: reportData.candidatesByProvince.map((p) => [p.province, p.count]),
      theme: 'striped',
      headStyles: { fillColor: PRIMARY, textColor: HEADER_TEXT, fontStyle: 'bold' },
      columnStyles: { 1: { halign: 'center' } },
      margin: { left: 14, right: 14 },
    });
    y = doc.lastAutoTable.finalY + 12;
  } else {
    doc.setFontSize(9);
    doc.setTextColor(120, 120, 120);
    doc.text('Sin candidatos registrados con provincia asignada.', 14, y);
    doc.setTextColor(0, 0, 0);
    y += 10;
  }

  // ── Sección 6: Vacantes por Provincia ────────────────────────────────────
  y = safeY(doc, y, 50);
  y = addSectionTitle(doc, 'SECCIÓN 6 — DISTRIBUCIÓN DE VACANTES POR PROVINCIA', y);
  if (reportData.vacantesByProvince.length > 0) {
    autoTable(doc, {
      startY: y,
      head: [['Provincia', 'Vacantes']],
      body: reportData.vacantesByProvince.map((p) => [p.province, p.count]),
      theme: 'striped',
      headStyles: { fillColor: PRIMARY, textColor: HEADER_TEXT, fontStyle: 'bold' },
      columnStyles: { 1: { halign: 'center' } },
      margin: { left: 14, right: 14 },
    });
    y = doc.lastAutoTable.finalY + 12;
  } else {
    doc.setFontSize(9);
    doc.setTextColor(120, 120, 120);
    doc.text('Sin vacantes registradas con provincia asignada.', 14, y);
    doc.setTextColor(0, 0, 0);
    y += 10;
  }

  // ── Sección 7: Microcursos ────────────────────────────────────────────────
  y = safeY(doc, y, 30);
  y = addSectionTitle(doc, 'SECCIÓN 7 — MICROCURSOS', y);
  doc.setFontSize(9);
  if (reportData.totalMicrocursos > 0) {
    doc.text(`Total de microcursos disponibles: ${reportData.totalMicrocursos}`, 14, y);
  } else {
    doc.setTextColor(120, 120, 120);
    doc.text('No existen microcursos registrados actualmente.', 14, y);
    doc.setTextColor(0, 0, 0);
  }
  y += 12;

  // ── Sección 8: Observaciones ──────────────────────────────────────────────
  y = safeY(doc, y, 30);
  y = addSectionTitle(doc, 'SECCIÓN 8 — OBSERVACIONES', y);
  doc.setFontSize(9);
  doc.setTextColor(80, 80, 80);
  doc.text(
    'Los datos mostrados corresponden a la información almacenada en la plataforma al momento de generar este reporte.',
    14,
    y,
    { maxWidth: pageW - 28 },
  );
  doc.setTextColor(0, 0, 0);

  // ── Pie de página en todas las páginas ───────────────────────────────────
  const pageCount = doc.getNumberOfPages();
  for (let i = 1; i <= pageCount; i++) {
    doc.setPage(i);
    doc.setFontSize(8);
    doc.setTextColor(150, 150, 150);
    doc.text(
      `Sinergia — Reporte generado el ${dateStr} | Página ${i} de ${pageCount}`,
      pageW / 2,
      doc.internal.pageSize.getHeight() - 8,
      { align: 'center' },
    );
  }

  const fileName = `reporte-sinergia-${now.toISOString().slice(0, 10)}.pdf`;
  doc.save(fileName);
}
